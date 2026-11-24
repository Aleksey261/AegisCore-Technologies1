async function loadHtml(url, targetId){const target=document.getElementById(targetId); if(!target) return; const r=await fetch(url); target.innerHTML=await r.text();}
