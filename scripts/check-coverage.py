import json
from pathlib import Path
root = Path(__file__).resolve().parents[1]
tools = json.loads((root / 'docs/native-catalog.json').read_text(encoding='utf-8-sig'))['tools']
names = {(r.get('category', ''), r['name']) for r in json.loads((root / 'Languages/zh-CN.json').read_text(encoding='utf-8'))}
descriptions = {r['name'] for r in json.loads((root / 'Languages/tool-text.zh-CN.json').read_text(encoding='utf-8'))}
params = [t for t in tools if t['category'] == 'Params']
missing = [t['name'] for t in params if ('Params', t['name']) not in names or t['description'] not in descriptions]
assert not missing, missing
report = {'scope': 'Extracted Params metadata only; not all native tools or GUI coverage', 'params_records': len(params), 'missing_params_names_or_descriptions': missing, 'all_extracted_records': len(tools), 'note': 'Core assembly export only; full catalog must be exported inside Rhino.'}
(root / 'docs/coverage.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(report))
