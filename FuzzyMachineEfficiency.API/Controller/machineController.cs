using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FuzzyMachineEfficiency.API.Controllers
{
    [Route("api/fuzzy")]
    [ApiController]
    public class FuzzyController : ControllerBase
    {
        public class IsEmri
        {
            public int UrunSayisi { get; set; }
            public int GunSayisi { get; set; }
        }

       
        public class MakineSonuc
        {
            public string MakineAdi { get; set; }
            public int Puan { get; set; }
            public string Durum { get; set; } 
            public string Renk { get; set; }
            public List<string> Aciklamalar { get; set; } = new List<string>();
        }

        private class MakineProfil
        {
            public string Ad { get; set; }
            public int GunlukMaxKapasite { get; set; } 
            public int EnerjiPuani { get; set; } 
            public int HataPuani { get; set; } 
        }

        private readonly List<MakineProfil> _makineler = new List<MakineProfil>
        {
            new MakineProfil { Ad = "A Grubu: CNC Lazer", GunlukMaxKapasite = 500, EnerjiPuani = 40, HataPuani = 95 },
            new MakineProfil { Ad = "B Grubu: Hidrolik", GunlukMaxKapasite = 800, EnerjiPuani = 50, HataPuani = 70 },
            new MakineProfil { Ad = "C Grubu: Eco-Robot", GunlukMaxKapasite = 300, EnerjiPuani = 95, HataPuani = 85 },
            new MakineProfil { Ad = "D Grubu: Manuel", GunlukMaxKapasite = 150, EnerjiPuani = 80, HataPuani = 60 }
        };

        [HttpPost("kiyasla")]
        public IActionResult Kiyasla([FromBody] IsEmri isEmri)
        {
            var sonuclar = new List<MakineSonuc>();

            double gerekenGunlukUretim = (double)isEmri.UrunSayisi / isEmri.GunSayisi;

            foreach (var makine in _makineler)
            {
                var analiz = MakineyiAnalizEt(makine, gerekenGunlukUretim);
                sonuclar.Add(analiz);
            }
            return Ok(sonuclar.OrderByDescending(x => x.Puan).ToList());
        }
        private MakineSonuc MakineyiAnalizEt(MakineProfil makine, double gerekenHiz)
        {
            double toplamPuan = 0;
            double toplamAgirlik = 0;
            var kurallar = new List<string>();
            double karsilamaOrani = makine.GunlukMaxKapasite / gerekenHiz;
            
            double yetersiz = SolOmuz(karsilamaOrani, 0.8, 1.0); 
            double tamUygun = Ucgen(karsilamaOrani, 0.9, 1.2, 2.0); 
            double fazlaGuclu = SagOmuz(karsilamaOrani, 2.0, 5.0); 

            void KuralEkle(double derece, double skor, string mesaj)
            {
                if (derece > 0)
                {
                    toplamAgirlik += derece;
                    toplamPuan += (derece * skor);
                    if (derece > 0.2) kurallar.Add(mesaj);
                }
            }
            KuralEkle(yetersiz, 10, "Kapasite yetersiz! İş yetişmeyebilir.");
            KuralEkle(tamUygun, 100, "Kapasite bu iş için ideal.");
            KuralEkle(fazlaGuclu, 60, "Makine bu iş için fazla güçlü (Atıl kapasite).");
            KuralEkle(0.5, makine.EnerjiPuani, "Makinenin enerji skoru yansıtıldı.");
            KuralEkle(0.5, makine.HataPuani, "Makinenin hassasiyet skoru yansıtıldı.");
            if (toplamAgirlik == 0) toplamAgirlik = 1; 
            double finalScore = toplamPuan / toplamAgirlik;
            if (karsilamaOrani < 1.0)
            {
                finalScore = Math.Min(finalScore, 40); 
                kurallar.Insert(0, $"GEREKLİ: {Math.Ceiling(gerekenHiz)}/gün, KAPASİTE: {makine.GunlukMaxKapasite}/gün");
            }
            string renk = "#95a5a6"; 
            string durum = "Belirsiz";
            if (finalScore >= 80) { renk = "#2ecc71"; durum = "ÇOK UYGUN"; }
            else if (finalScore >= 60) { renk = "#f1c40f"; durum = "UYGUN / KABUL EDİLEBİLİR"; }
            else if (finalScore >= 40) { renk = "#e67e22"; durum = "VERİMSİZ"; }
            else { renk = "#c0392b"; durum = "YETERSİZ / RİSKLİ"; }

            return new MakineSonuc
            {
                MakineAdi = makine.Ad,
                Puan = (int)Math.Round(finalScore),
                Durum = durum,
                Renk = renk,
                Aciklamalar = kurallar
            };
        }
        private double SolOmuz(double x, double a, double b)
        {
            if (x <= a) return 1.0;
            if (x >= b) return 0.0;
            return (b - x) / (b - a);
        }
        private double SagOmuz(double x, double a, double b)
        {
            if (x <= a) return 0.0;
            if (x >= b) return 1.0;
            return (x - a) / (b - a);
        }
        private double Ucgen(double x, double a, double b, double c)
        {
            if (x <= a || x >= c) return 0.0;
            if (x == b) return 1.0;
            if (x < b) return (x - a) / (b - a);
            return (c - x) / (c - b);
        }
    }
}