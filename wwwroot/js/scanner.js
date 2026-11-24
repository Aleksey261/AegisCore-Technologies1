async function startScan(){
  const target=document.getElementById('scanTarget')?.value || '127.0.0.1';
  const out=document.getElementById('scanResult'); out.textContent='Сканирование запущено...';
  const r=await fetch('/scanner/start?target='+encodeURIComponent(target),{method:'POST'});
  const j=await r.json(); out.textContent=JSON.stringify(j,null,2); await loadTab('/scanner/scans');
}
async function assessScan(){
  const out=document.getElementById('scanResult'); out.textContent='Проверка по ФСТЭК...';
  const r=await fetch('/scanner/assess',{method:'POST'}); const j=await r.json(); out.textContent=JSON.stringify(j,null,2);
}
async function loadTab(url){ await loadHtml(url,'moduleContent'); }
document.addEventListener('DOMContentLoaded',()=>loadTab('/scanner/scans'));
