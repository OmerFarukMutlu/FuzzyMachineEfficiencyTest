async function analizEt() {
    const adet = document.getElementById("inputAdet").value;
    const gun = document.getElementById("inputGun").value;
    const resultsArea = document.getElementById("resultsArea");
    const resultsGrid = document.getElementById("resultsGrid");

    if (!adet || !gun || adet <= 0 || gun <= 0) {
        alert("Lütfen geçerli üretim adetleri ve gün sayısı giriniz.");
        return;
    }

    const requestData = {
        UrunSayisi: parseInt(adet),
        GunSayisi: parseInt(gun)
    };

    resultsArea.style.display = "block";
    resultsGrid.innerHTML = "<p>Analiz yapılıyor, lütfen bekleyin...</p>";

    try {
        const url = "http://localhost:5141/api/fuzzy/kiyasla"; 
        
        const response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(requestData)
        });

        if (!response.ok) throw new Error("Sunucu Hatası");

        const data = await response.json();
        
        resultsGrid.innerHTML = "";

        data.forEach(item => {
            const card = document.createElement("div");
            card.className = "machine-card";
            card.style.borderLeftColor = item.renk; 

            
            card.innerHTML = `
                <h3 style="color:${item.renk}">${item.makineAdi}</h3>
                <div class="score-display">
                    <span style="font-size: 2em; font-weight:bold">${item.puan}</span>
                    <small>/100</small>
                </div>
                <p><strong>Durum:</strong> ${item.durum}</p>
            `;
            resultsGrid.appendChild(card);
        });

    } catch (error) {
        console.error(error);
        resultsGrid.innerHTML = `<p style="color:red">Hata: ${error.message}. Backend'in çalıştığından emin olun.</p>`;
    }
}