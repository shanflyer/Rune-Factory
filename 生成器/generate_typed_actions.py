"""
GameAction 强类型 ScriptableObject 代码生成器 v4
修复: Init() brace 计数提取、struct 字段白名单过滤、覆盖逻辑
"""
import os, re
from pathlib import Path

ACTION_DIR = Path(__file__).parent.parent / "Assets" / "Scripts" / "Action"
OUTPUT_DIR = ACTION_DIR / "TypedActions"
OUTPUT_DIR.mkdir(exist_ok=True)

EXCLUDE_FIELDS = {"setValue", "setResult", "endAction"}
CSHARP_TYPES = {"int", "bool", "string", "float", "double", "long", "char", "byte",
                "Vector2", "Vector3", "Vector2Int", "Vector3Int", "Action"}
SHADOWED_FIELDS = {"id", "name"}  # 基类已有，生成时加 new

def extract_brace_block(text, start):
    depth, i = 0, start
    while i < len(text):
        if text[i] == '{': depth += 1
        elif text[i] == '}':
            depth -= 1
            if depth == 0:
                return text[start+1:i], i+1
        i += 1
    return "", start

def extract_structs_from_file(content):
    results = []
    pattern = re.compile(r'public\s+struct\s+(\w+)\s*:\s*GameAction')
    pos = 0
    while pos < len(content):
        m = pattern.search(content, pos)
        if not m: break
        name = m.group(1)
        if name == "GameAction":
            pos = m.end(); continue
        brace_start = content.find('{', m.end())
        if brace_start < 0:
            pos = m.end(); continue
        body, end_pos = extract_brace_block(content, brace_start)
        if body:
            results.append((name, body))
        pos = end_pos
    return results

def extract_init_body(struct_body):
    """用 brace 计数提取 Init() 方法体"""
    init_sig = re.search(r'public\s+void\s+Init\s*\([^)]*\)\s*\{', struct_body)
    if not init_sig:
        return None
    brace_start = init_sig.end() - 1  # 指向 {
    body, _ = extract_brace_block(struct_body, brace_start)
    return body

def extract_struct_fields(struct_body):
    """从 struct body 提取字段，跳过注释行"""
    fields = {}
    lines = struct_body.split('\n')
    active_lines = []
    for line in lines:
        stripped = line.strip()
        # 跳过单行注释
        if stripped.startswith('//'):
            # 保留被注释掉的字段作为已排除标记
            continue
        active_lines.append(line)
    
    clean_body = '\n'.join(active_lines)
    
    for m in re.finditer(r'public\s+(\S+)\s+(\w+)\s*;', clean_body):
        ftype, fname = m.group(1), m.group(2)
        if fname not in EXCLUDE_FIELDS and not fname.startswith('_') and '<' not in ftype and ftype != "Action":
            st = simplify_type(ftype)
            if st:
                fields[fname] = st
    for m in re.finditer(r'public\s+(\S+)\s+((?:\w+\s*,?\s*)+)\s*;', clean_body):
        ftype = m.group(1)
        if '<' in ftype or ftype == "Action":
            continue
        st = simplify_type(ftype)
        if not st:
            continue
        for fname in m.group(2).split(','):
            fname = fname.strip().split('=')[0].strip()
            if fname and fname not in EXCLUDE_FIELDS and fname not in CSHARP_TYPES:
                fields[fname] = st
    return fields

SIMPLE_TYPES = {"int", "bool", "string", "float", "Vector2", "Vector3", "Vector2Int", "Vector3Int"}

def simplify_type(ftype):
    """返回 None 表示跳过（复杂类型，由原始 Init() 处理）"""
    if ftype in SIMPLE_TYPES:
        return ftype
    return None

def parse_init(init_body, struct_field_names):
    result = {"param_fields": [], "source_overrides": [], "target_overrides": [], "value_overrides": []}
    
    # (1) 字段映射 — 只保留 struct 中真实存在的字段
    patterns = [
        (r'(\w+)\s*=\s*int\.Parse\s*\(\s*parameters\s*\[\s*(\d+)\s*\]\s*\.value\s*\)', "int"),
        (r'(\w+)\s*=\s*bool\.Parse\s*\(\s*parameters\s*\[\s*(\d+)\s*\]\s*\.value\s*\)', "bool"),
        (r'(\w+)\s*=\s*float\.Parse\s*\(\s*parameters\s*\[\s*(\d+)\s*\]\s*\.value\s*\)', "float"),
        (r'(?<!\w\s)(\w+)\s*=\s*parameters\s*\[\s*(\d+)\s*\]\s*\.value(?!\s*\.)', "string"),
    ]
    
    seen = set()
    for pattern, ptype in patterns:
        for m in re.findall(pattern, init_body):
            field, idx = m[0], m[1]
            # 白名单：必须是 struct 声明过的字段
            if field not in struct_field_names:
                continue
            if field not in seen:
                result["param_fields"].append((field, int(idx), ptype))
                seen.add(field)
    
    # (2) source/target/value 覆盖 — 同样白名单
    for fname in set(re.findall(r'(\w+)\s*=\s*source\s*;', init_body)):
        if fname in struct_field_names and fname not in EXCLUDE_FIELDS:
            result["source_overrides"].append(fname)
    for fname in set(re.findall(r'(\w+)\s*=\s*target\s*;', init_body)):
        if fname in struct_field_names and fname not in EXCLUDE_FIELDS:
            result["target_overrides"].append(fname)
    for fname in set(re.findall(r'(\w+)\s*=\s*value\s*;', init_body)):
        if fname in struct_field_names and fname not in EXCLUDE_FIELDS:
            result["value_overrides"].append(fname)
    
    return result

def generate_subclass(struct_name, fields, init_info):
    param_fields = sorted(init_info["param_fields"], key=lambda x: x[1])
    src_overrides = init_info["source_overrides"]
    tgt_overrides = init_info["target_overrides"]
    val_overrides = init_info["value_overrides"]
    
    used_names = set()
    field_decls, field_inits = [], []
    
    for pf_name, pf_idx, pf_type in param_fields:
        # 以 struct 实际声明的字段类型为准（Init() 正则可能误判）
        actual_type = fields.get(pf_name, pf_type)
        ct_map = {"int":"int","bool":"bool","float":"float","string":"string"}
        ct = ct_map.get(actual_type, "string")
        prefix = "new " if pf_name in SHADOWED_FIELDS else ""
        field_decls.append(f'        public {prefix}{ct} {pf_name};')
        field_inits.append(f'                {pf_name} = this.{pf_name}')
        used_names.add(pf_name)

    for fname, ftype in fields.items():
        if fname not in used_names:
            prefix = "new " if fname in SHADOWED_FIELDS else ""
            field_decls.append(f'        public {prefix}{ftype} {fname};')
            field_inits.append(f'                {fname} = this.{fname}')
            used_names.add(fname)
    
    override_lines = []
    for f in src_overrides:
        override_lines.append(f'            if (source != 0 && source != int.MinValue) action.{f} = source;')
    for f in tgt_overrides:
        override_lines.append(f'            if (target != 0 && target != int.MinValue) action.{f} = target;')
    for f in val_overrides:
        override_lines.append(f'            if (value != -1) action.{f} = value;')
    
    override_block = "\n".join(override_lines)
    if override_block:
        override_block = "\n" + override_block + "\n"
    
    field_section = "\n".join(field_decls) if field_decls else ""
    init_section = ",\n".join(field_inits) + ("\n        " if field_inits else "")
    
    code = f'''// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - {struct_name}
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/{struct_name}")]
public class {struct_name}ActionData : GameActionBaseData
{{
{field_section}

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {{
        var action = new {struct_name}
        {{
{init_section}
        }};

        action.setValue = setValue;
        action.setResult = setResult;{override_block}
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }}
}}
'''
    return code

def main():
    all_cs_files = list(ACTION_DIR.glob("*.cs"))
    exclude_files = {"GameActionBaseData.cs", "GameActionManager.cs",
                     "GameActionDataManager.cs", "GameActionData.cs"}
    
    generated = 0
    errors = []
    
    for cs_file in sorted(all_cs_files):
        if cs_file.name in exclude_files:
            continue
        
        with open(cs_file, "r", encoding="utf-8") as f:
            content = f.read()
        
        structs = extract_structs_from_file(content)
        
        for struct_name, struct_body in structs:
            if struct_name == "GameAction":
                continue
            
            init_body = extract_init_body(struct_body)
            fields = extract_struct_fields(struct_body)
            field_names = set(fields.keys())
            
            init_info = {"param_fields": [], "source_overrides": [],
                        "target_overrides": [], "value_overrides": []}
            if init_body:
                init_info = parse_init(init_body, field_names)
            
            try:
                code = generate_subclass(struct_name, fields, init_info)
            except Exception as e:
                errors.append(f"  {struct_name}: {e}")
                continue
            
            out_file = OUTPUT_DIR / f"{struct_name}ActionData.cs"
            with open(out_file, "w", encoding="utf-8", newline="\n") as f:
                f.write(code)
            
            generated += 1
            status = ""
            if not init_info["param_fields"] and not fields:
                status = " (容器)"
            elif init_info["source_overrides"] or init_info["target_overrides"]:
                status = f" (覆盖:s={len(init_info['source_overrides'])} t={len(init_info['target_overrides'])})"
            print(f"  + {struct_name}{status}")
    
    print(f"\n{'='*50}")
    print(f"  生成完成: {generated} 个类")
    if errors:
        for e in errors: print(f"  ! {e}")
    print(f"  输出: {OUTPUT_DIR}")
    print(f"{'='*50}")

if __name__ == "__main__":
    main()
