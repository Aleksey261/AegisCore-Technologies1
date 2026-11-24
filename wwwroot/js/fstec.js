async function importFstec(){
  const file=document.getElementById('fstecFile').files[0]; const out=document.getElementById('fstecResult');
  if(!file){out.textContent='Выберите XML или CSV файл'; return;}
  const fd=new FormData(); fd.append('file',file);
  const r=await fetch('/api/fstec/import',{method:'POST',body:fd}); out.textContent=JSON.stringify(await r.json(),null,2); await loadFstecLists();
}
async function assessFstec(){const r=await fetch('/api/fstec/assess',{method:'POST'}); document.getElementById('fstecResult').textContent=JSON.stringify(await r.json(),null,2);}
async function list(url,id,fields){const r=await fetch(url); const data=await r.json(); const rows=data.slice(0,10).map(x=>'<tr>'+fields.map(f=>'<td>'+(x[f]??'')+'</td>').join('')+'</tr>').join(''); document.getElementById(id).innerHTML='<table>'+rows+'</table><p>Всего: '+data.length+'</p>';}
async function loadFstecLists(){await list('/api/fstec/vulnerabilities','vulns',['bduId','cveId','severity','product']); await list('/api/fstec/threats','threats',['code','severity','name']); await list('/api/fstec/industrial','ics',['assetType','riskLevel','name']); await list('/api/fstec/export-control','export',['documentNumber','status','productName']);}
document.addEventListener('DOMContentLoaded',loadFstecLists);
