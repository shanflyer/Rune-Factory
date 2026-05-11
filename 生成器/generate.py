"""GameAction codegen v8 - Fixed: containers = GameActionData shell, typed = GameActionBaseData"""

import os, re
from pathlib import Path

AD = Path(r'D:\MyGame\Rune Factory\Rune Factory - Copy - 副本\Assets\Scripts\Action')
OD = AD / "TypedNodes"
OD.mkdir(exist_ok=True)
EX = {"setValue", "setResult", "endAction"}
ST = {"int", "bool", "string", "float", "Vector2", "Vector3", "Vector2Int", "Vector3Int"}
SD = {"id", "name"}

def bb(t, s):
    d, i = 0, s
    while i < len(t):
        if t[i] == '{': d += 1
        elif t[i] == '}':
            d -= 1
            if d == 0: return t[s+1:i]
        i += 1
    return t[s+1:]

def xs(c):
    r, p = [], 0
    pat = re.compile(r'public\s+struct\s+(\w+)\s*:\s*GameAction')
    while p < len(c):
        m = pat.search(c, p)
        if not m: break
        n = m.group(1)
        if n == "GameAction": p = m.end(); continue
        b = c.find('{', m.end())
        if b < 0: p = m.end(); continue
        body = bb(c, b)
        if body: r.append((n, body))
        p = b + len(body) + 2
    return r

def sim(t): return t if t in ST else None

def sf(body):
    f = {}
    cl = '\n'.join(l for l in body.split('\n') if not l.strip().startswith('//'))
    for m in re.finditer(r'public\s+(\S+)\s+(\w+)\s*;', cl):
        ft, fn = m.group(1), m.group(2)
        if fn not in EX and not fn.startswith('_') and '<' not in ft and ft != "Action":
            s = sim(ft)
            if s: f[fn] = s
    return f

def ib(body):
    m = re.search(r'public\s+void\s+Init\s*\([^)]*\)\s*\{', body)
    return bb(body, m.end() - 1) if m else None

def pi(ib, fnames):
    r = {"pf": [], "src": [], "tgt": [], "val": []}
    ps = [
        (r'(\w+)\s*=\s*int\.Parse\s*\(\s*parameters\s*\[(\d+)\]\s*\.value\s*\)', "int"),
        (r'(\w+)\s*=\s*bool\.Parse\s*\(\s*parameters\s*\[(\d+)\]\s*\.value\s*\)', "bool"),
        (r'(\w+)\s*=\s*float\.Parse\s*\(\s*parameters\s*\[(\d+)\]\s*\.value\s*\)', "float"),
        (r'(\w+)\s*=\s*parameters\s*\[(\d+)\]\s*\.value', "string"),
    ]
    seen = set()
    for p, t in ps:
        for m in re.findall(p, ib):
            f, idx = m[0], m[1]
            if f in fnames and f not in seen:
                r["pf"].append((f, int(idx), t)); seen.add(f)
    for f in set(re.findall(r'(\w+)\s*=\s*source\s*;', ib)):
        if f in fnames and f not in EX: r["src"].append(f)
    for f in set(re.findall(r'(\w+)\s*=\s*target\s*;', ib)):
        if f in fnames and f not in EX: r["tgt"].append(f)
    for f in set(re.findall(r'(\w+)\s*=\s*value\s*;', ib)):
        if f in fnames and f not in EX: r["val"].append(f)
    return r

def gen_container(name):
    lines = []
    lines.append("// container - " + name)
    lines.append("using System;")
    lines.append("using UnityEngine;")
    lines.append("")
    # no Serializable - base class has it
    lines.append("public class " + name + "Node : ContainerNode")
    lines.append("{")
    lines.append("    public override GameAction CreateAction(")
    lines.append("        int source = 0, int target = 0, int value = -1,")
    lines.append("        SetResult setResult = null, SetValue setValue = null,")
    lines.append("        bool immediately = false)")
    lines.append("    {")
    lines.append("        var action = new " + name + " { setValue = setValue, setResult = setResult };")
    lines.append("        ExecuteChildren(source, target, value, setResult, setValue, immediately);")
    lines.append("        GameActionManager.instance.QueueAction(action, immediately);")
    lines.append("        return action;")
    lines.append("    }")
    lines.append("}")
    return '\n'.join(lines)

def gen_leaf(name, fields, info):
    pf = sorted(info["pf"], key=lambda x: x[1])
    src, tgt, val = info["src"], info["tgt"], info["val"]
    used, decls, inits = set(), [], []
    for fn, fi, ft in pf:
        at = fields.get(fn, ft)
        ct = {"int":"int","bool":"bool","float":"float","string":"string"}.get(at, "string")
        kw = "new " if fn in SD else ""
        decls.append("        public " + kw + ct + " " + fn + ";")
        inits.append("                " + fn + " = this." + fn)
        used.add(fn)
    for fn, ft in fields.items():
        if fn not in used:
            kw = "new " if fn in SD else ""
            decls.append("        public " + kw + ft + " " + fn + ";")
            inits.append("                " + fn + " = this." + fn)
            used.add(fn)
    ovr = []
    for f in src:
        ovr.append("            if (source != 0 && source != int.MinValue) action." + f + " = source;")
    for f in tgt:
        ovr.append("            if (target != 0 && target != int.MinValue) action." + f + " = target;")
    for f in val:
        ovr.append("            if (value != -1) action." + f + " = value;")
    ob = '\n'.join(ovr)
    if ob: ob = "\n" + ob + "\n"

    lines = []
    lines.append("// " + name)
    lines.append("using System;")
    lines.append("using UnityEngine;")
    lines.append("")
    # no Serializable - base class has it
    lines.append("public class " + name + "Node : ActionNode")
    lines.append("{")
    for d in decls: lines.append(d)
    lines.append("")
    lines.append("    public override GameAction CreateAction(")
    lines.append("        int source = 0, int target = 0, int value = -1,")
    lines.append("        SetResult setResult = null, SetValue setValue = null,")
    lines.append("        bool immediately = false)")
    lines.append("    {")
    lines.append("        var action = new " + name)
    lines.append("        {")
    for init in inits: lines.append(init + ",")
    lines.append("        };")
    if ob.strip():
        lines.append("        action.setValue = setValue;")
        lines.append("        action.setResult = setResult;" + ob + "        GameActionManager.instance.QueueAction(action, immediately);")
    else:
        lines.append("        action.setValue = setValue;")
        lines.append("        action.setResult = setResult;")
        lines.append("        GameActionManager.instance.QueueAction(action, immediately);")
    lines.append("        return action;")
    lines.append("    }")
    lines.append("}")
    return '\n'.join(lines)

def main():
    files = sorted(AD.glob("*.cs"))
    ex = {"GameActionBaseData.cs", "GameActionManager.cs", "GameActionDataManager.cs", "GameActionData.cs", "ActionNode.cs"}
    g = 0
    for f in files:
        if f.name in ex: continue
        c = open(f, 'r', encoding='utf-8').read()
        for n, b in xs(c):
            fields = sf(b)
            ibd = ib(b)
            info = pi(ibd, set(fields.keys())) if ibd else {"pf":[], "src":[], "tgt":[], "val":[]}
            isc = not any(fields.values()) and not info["pf"]
            code = gen_container(n) if isc else gen_leaf(n, fields, info)
            out = OD / (n + "Node.cs")
            with open(out, 'w', encoding='utf-8', newline='\n') as fp: fp.write(code)
            g += 1
            tag = "[GameActionData]" if isc else ""
            print("  + " + n + "ActionData.cs " + tag)
    print("  Done: " + str(g) + " classes")

if __name__ == "__main__":
    main()
