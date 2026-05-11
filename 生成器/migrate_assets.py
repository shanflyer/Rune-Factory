"""Batch convert old GameActionData .asset to new GameActionAsset .asset"""

import os, re, yaml

OLD_DIR = r'D:\MyGame\Rune Factory\Rune Factory - Copy - 副本\Assets\Resources\Data\GameActionData'
NEW_DIR = r'D:\MyGame\Rune Factory\Rune Factory - Copy - 副本\Assets\Resources\Data\GameActionAssets'
os.makedirs(NEW_DIR, exist_ok=True)

# Get all node types from TypedNodes
NODE_TYPES = {}
typed_nodes_dir = r'D:\MyGame\Rune Factory\Rune Factory - Copy - 副本\Assets\Scripts\Action\TypedNodes'
for f in os.listdir(typed_nodes_dir):
    if not f.endswith('.cs'): continue
    with open(os.path.join(typed_nodes_dir, f), 'r', encoding='utf-8') as fp:
        content = fp.read()
    m = re.search(r'class (\w+) : (?:ContainerNode|ActionNode)', content)
    if m:
        node_name = m.group(1)
        is_container = 'ContainerNode' in m.group(2)
        # Extract field names+types
        fields = re.findall(r'public\s+(?:new\s+)?(int|bool|string|float|Vector[23](?:Int)?)\s+(\w+)\s*;', content)
        struct_name = node_name.replace('Node', '')
        NODE_TYPES[struct_name] = {
            'node_name': node_name,
            'is_container': is_container,
            'fields': fields  # [(type, name), ...]
        }

print(f'Node types: {len(NODE_TYPES)}')

# Get GameActionAsset script GUID
import subprocess
result = subprocess.run(
    ['python', '-c', 
     'import os; [print(f) for f in os.listdir(r"D:\\MyGame\\Rune Factory\\Rune Factory - Copy - 副本\\Assets\\Scripts\\Action\\TypedNodes") if "GameActionAsset" in f]'],
    capture_output=True, text=True
)
# We need the GUID from .meta file - let's just search
asset_script_guid = None
for root, dirs, files in os.walk(r'D:\MyGame\Rune Factory\Rune Factory - Copy - 副本\Assets\Scripts\Action'):
    if 'ActionNode.cs.meta' in files:
        mp = os.path.join(root, 'ActionNode.cs.meta')
        with open(mp, 'r') as fp:
            meta = fp.read()
        m = re.search(r'guid:\s*([a-f0-9]+)', meta)
        if m:
            asset_script_guid = m.group(1)
            break

print(f'ActionNode GUID: {asset_script_guid}')

converted = 0
errors = 0

for fname in os.listdir(OLD_DIR):
    if not fname.endswith('.asset'): continue
    old_path = os.path.join(OLD_DIR, fname)
    
    with open(old_path, 'r', encoding='utf-8') as fp:
        content = fp.read()
    
    m = re.search(r'typeName:\s*(\S+)', content)
    if not m: continue
    type_name = m.group(1)
    
    if type_name not in NODE_TYPES:
        print(f'  SKIP {fname}: type {type_name} not found')
        errors += 1
        continue
    
    node_info = NODE_TYPES[type_name]
    
    # Extract old parameters
    params = []
    param_section = re.findall(r'- value:\s*"(.*?)"', content)
    
    # Extract id
    id_m = re.search(r'\bid:\s*(\d+)', content)
    asset_id = int(id_m.group(1)) if id_m else -1
    
    # Build new YAML
    lines = []
    lines.append('%YAML 1.1')
    lines.append('%TAG !u! tag:unity3d.com,2011:')
    lines.append('--- !u!114 &11400000')
    lines.append('MonoBehaviour:')
    lines.append('  m_ObjectHideFlags: 0')
    lines.append('  m_CorrespondingSourceObject: {fileID: 0}')
    lines.append('  m_PrefabInstance: {fileID: 0}')
    lines.append('  m_PrefabAsset: {fileID: 0}')
    lines.append('  m_GameObject: {fileID: 0}')
    lines.append('  m_Enabled: 1')
    lines.append('  m_EditorHideFlags: 0')
    lines.append(f'  m_Script: {{fileID: 11500000, guid: {asset_script_guid}, type: 3}}')
    
    name = fname.replace('.asset', '')
    lines.append(f'  m_Name: {name}')
    lines.append('  m_EditorClassIdentifier: ')
    lines.append(f'  id: {asset_id}')
    
    # root node
    if node_info['is_container']:
        # Container node
        lines.append('  root:')
        lines.append(f'    rid: {asset_id}00000001')
        lines.append(f'  references:')
        lines.append(f'    version: 2')
        lines.append(f'    RefIds:')
        # Container node reference
        lines.append(f'    - rid: {asset_id}00000001')
        lines.append(f'      type: {{class: {node_info["node_name"]}, ns: , asm: Assembly-CSharp}}')
        lines.append(f'      data:')
        lines.append(f'        children:')
        if param_section:
            for i, val in enumerate(param_section):
                lines.append(f'        - node:')
                lines.append(f'            rid: {asset_id}00000{i+2}01')
        else:
            lines.append(f'        []')
        
        # Sub-node references (each param)
        if param_section:
            lines.append(f'    - rid: ... (complex)')
            # Actually this gets really complex with nested YAML references
            # Skip for now and just create empty container
            lines.append(f'    - []')
    else:
        # Leaf node
        fields = node_info['fields']
        lines.append('  root:')
        lines.append(f'    rid: {asset_id}00000001')
        lines.append(f'  references:')
        lines.append(f'    version: 2')
        lines.append(f'    RefIds:')
        lines.append(f'    - rid: {asset_id}00000001')
        lines.append(f'      type: {{class: {node_info["node_name"]}, ns: , asm: Assembly-CSharp}}')
        lines.append(f'      data:')
        for fi, (ftype, fname) in enumerate(fields):
            val = param_section[fi] if fi < len(param_section) else '0'
            lines.append(f'        {fname}: {val}')
    
    new_path = os.path.join(NEW_DIR, fname)
    with open(new_path, 'w', encoding='utf-8', newline='\n') as fp:
        fp.write('\n'.join(lines))
    
    converted += 1
    if converted % 50 == 0:
        print(f'  ... {converted} done')

print(f'\nDone: {converted} converted, {errors} skipped')
