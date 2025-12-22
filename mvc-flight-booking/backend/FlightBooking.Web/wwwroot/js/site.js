// Site genelinde kullanılan JavaScript fonksiyonları

// Sayfa yüklendiğinde çalışacak fonksiyonlar
document.addEventListener('DOMContentLoaded', function() {
    // Tüm interaktif elementleri başlat
    initializeNavigation(); // Navigasyon menüsünü başlat
    initializeForms(); // Form özelliklerini başlat
    initializeAnimations(); // Animasyonları başlat
    initializeTooltips(); // Tooltip'leri başlat
});

// Navigasyon menüsü fonksiyonları
function initializeNavigation() {
    // Mobil menü toggle butonu (gelecekte kullanılabilir)
    const navbarToggle = document.querySelector('.navbar-toggle');
    if (navbarToggle) {
        navbarToggle.addEventListener('click', function() {
            const navbarLinks = document.querySelector('.navbar-links');
            navbarLinks.classList.toggle('active');
        });
    }

    // Anchor linkleri için yumuşak kaydırma efekti
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth', // Yumuşak kaydırma
                    block: 'start' // Sayfanın başına kaydır
                });
            }
        });
    });
}

// Form geliştirmeleri ve validasyonlar
function initializeForms() {
    // Formlara yükleme durumu ekle
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = form.querySelector('button[type="submit"], input[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                submitBtn.disabled = true; // Butonu devre dışı bırak
                const originalText = submitBtn.textContent || submitBtn.value;
                submitBtn.textContent = 'İşleniyor...'; // Buton metnini değiştir
                submitBtn.classList.add('loading'); // Yükleme animasyonu ekle
                
                // 5 saniye sonra butonu tekrar aktif et (güvenlik önlemi)
                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.textContent = originalText;
                    submitBtn.classList.remove('loading');
                }, 5000);
            }
        });
    });

    // Input alanlarına focus efekti ekle
    document.querySelectorAll('input, select, textarea').forEach(input => {
        input.addEventListener('focus', function() {
            this.parentElement.classList.add('focused'); // Focus sınıfı ekle
        });
        
        input.addEventListener('blur', function() {
            this.parentElement.classList.remove('focused'); // Focus sınıfını kaldır
        });
    });

    // Telefon numaralarını otomatik formatla
    document.querySelectorAll('input[type="tel"]').forEach(input => {
        input.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, ''); // Sadece rakamları al
            // Telefon formatı: (555) 123-4567
            if (value.length > 0) {
                if (value.length <= 3) {
                    value = `(${value}`;
                } else if (value.length <= 6) {
                    value = `(${value.slice(0, 3)}) ${value.slice(3)}`;
                } else if (value.length <= 10) {
                    value = `(${value.slice(0, 3)}) ${value.slice(3, 6)}-${value.slice(6)}`;
                } else {
                    value = `(${value.slice(0, 3)}) ${value.slice(3, 6)}-${value.slice(6, 10)}`;
                }
            }
            e.target.value = value;
        });
    });
}

// Animasyon yardımcı fonksiyonları
function initializeAnimations() {
    // Intersection Observer - Sayfa kaydırıldığında elementleri animasyonlu göster
    const observerOptions = {
        threshold: 0.1, // Elementin %10'u görünür olduğunda tetikle
        rootMargin: '0px 0px -50px 0px' // Alt kısımdan 50px önce tetikle
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in'); // Animasyon sınıfı ekle
            }
        });
    }, observerOptions);

    // Tüm kartları ve bölümleri gözlemle
    document.querySelectorAll('.card, .flight-card, .booking-card').forEach(el => {
        observer.observe(el);
    });
}

// Tooltip (ipucu baloncuğu) fonksiyonları
function initializeTooltips() {
    // Basit tooltip implementasyonu
    document.querySelectorAll('[data-tooltip]').forEach(element => {
        // Mouse üzerine geldiğinde tooltip göster
        element.addEventListener('mouseenter', function() {
            const tooltip = document.createElement('div');
            tooltip.className = 'tooltip';
            tooltip.textContent = this.getAttribute('data-tooltip');
            document.body.appendChild(tooltip);
            
            // Tooltip pozisyonunu hesapla (elementin üstünde ortalı)
            const rect = this.getBoundingClientRect();
            tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
            tooltip.style.top = rect.top - tooltip.offsetHeight - 10 + 'px';
            
            this._tooltip = tooltip;
        });
        
        // Mouse ayrıldığında tooltip'i kaldır
        element.addEventListener('mouseleave', function() {
            if (this._tooltip) {
                document.body.removeChild(this._tooltip);
                this._tooltip = null;
            }
        });
    });
}

// Yardımcı fonksiyonlar
// Kullanıcıya bildirim göster
function showAlert(message, type = 'info') {
    const alert = document.createElement('div');
    alert.className = `alert alert-${type}`;
    alert.textContent = message;
    alert.style.position = 'fixed';
    alert.style.top = '20px';
    alert.style.right = '20px';
    alert.style.zIndex = '9999';
    alert.style.maxWidth = '400px';
    
    document.body.appendChild(alert);
    
    // 5 saniye sonra otomatik kaldır
    setTimeout(() => {
        if (alert.parentNode) {
            alert.style.opacity = '0';
            setTimeout(() => {
                document.body.removeChild(alert);
            }, 300);
        }
    }, 5000);
    
    // Tıklayınca kapat
    alert.addEventListener('click', () => {
        if (alert.parentNode) {
            document.body.removeChild(alert);
        }
    });
}

// Para birimi formatla (TL)
function formatCurrency(amount) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(amount);
}

// Tarih formatla (Türkçe)
function formatDate(date) {
    return new Intl.DateTimeFormat('tr-TR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(date));
}

// Saat formatla (24 saat formatı)
function formatTime(time) {
    return new Intl.DateTimeFormat('tr-TR', {
        hour: '2-digit',
        minute: '2-digit'
    }).format(new Date(time));
}

// Uçuş arama formu validasyonu
function validateFlightSearch(form) {
    const departure = form.querySelector('[name="departureCity"]').value;
    const arrival = form.querySelector('[name="arrivalCity"]').value;
    const date = form.querySelector('[name="departureDate"]').value;
    
    // Tüm alanların dolu olup olmadığını kontrol et
    if (!departure || !arrival || !date) {
        showAlert('Lütfen tüm alanları doldurun.', 'warning');
        return false;
    }
    
    // Kalkış ve varış şehri aynı olamaz
    if (departure === arrival) {
        showAlert('Kalkış ve varış şehri aynı olamaz.', 'warning');
        return false;
    }
    
    // Geçmiş tarih seçilemez
    const selectedDate = new Date(date);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    if (selectedDate < today) {
        showAlert('Geçmiş bir tarih seçemezsiniz.', 'warning');
        return false;
    }
    
    return true;
}

// Rezervasyon formu validasyonu
function validateBookingForm(form) {
    const requiredFields = form.querySelectorAll('[required]');
    let isValid = true;
    
    // Zorunlu alanları kontrol et
    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            field.classList.add('error'); // Hata sınıfı ekle
            isValid = false;
        } else {
            field.classList.remove('error'); // Hata sınıfını kaldır
        }
    });
    
    // TC Kimlik numaralarını doğrula
    const tcFields = form.querySelectorAll('[data-tc-validate]');
    tcFields.forEach(field => {
        if (field.value && !validateTCKimlik(field.value)) {
            field.classList.add('error');
            showAlert('Geçersiz TC Kimlik numarası.', 'error');
            isValid = false;
        }
    });
    
    return isValid;
}

// TC Kimlik numarası doğrulama algoritması
function validateTCKimlik(tc) {
    // 11 haneli ve sadece rakam olmalı
    if (!/^\d{11}$/.test(tc)) return false;
    // İlk hane 0 olamaz
    if (tc[0] === '0') return false;
    
    const digits = tc.split('').map(Number);
    // 10. hane kontrolü: (1+3+5+7+9. hanelerin toplamı * 7) - (2+4+6+8. hanelerin toplamı) mod 10
    const sum1 = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
    const sum2 = digits[1] + digits[3] + digits[5] + digits[7];
    
    if ((sum1 * 7 - sum2) % 10 !== digits[9]) return false;
    // 11. hane kontrolü: İlk 10 hanenin toplamı mod 10
    if ((sum1 + sum2 + digits[9]) % 10 !== digits[10]) return false;
    
    return true;
}

// PNR (Rezervasyon Kodu) yardımcı fonksiyonları
// PNR'yi formatla: Büyük harf ve sadece harf/rakam
function formatPNR(pnr) {
    return pnr.toUpperCase().replace(/[^A-Z0-9]/g, '');
}

// PNR doğrulama: 6 karakter, sadece harf ve rakam
function validatePNR(pnr) {
    const formatted = formatPNR(pnr);
    return formatted.length === 6 && /^[A-Z0-9]{6}$/.test(formatted);
}

// Yükleme durumu göster/gizle
function showLoading(element) {
    element.classList.add('loading'); // Yükleme animasyonu ekle
    element.disabled = true; // Elementi devre dışı bırak
}

function hideLoading(element) {
    element.classList.remove('loading'); // Yükleme animasyonunu kaldır
    element.disabled = false; // Elementi aktif et
}

// Fonksiyonları global olarak erişilebilir yap
// Diğer JavaScript dosyalarından kullanılabilir
window.FlightBooking = {
    showAlert,
    formatCurrency,
    formatDate,
    formatTime,
    validateFlightSearch,
    validateBookingForm,
    validateTCKimlik,
    formatPNR,
    validatePNR,
    showLoading,
    hideLoading
};

// Animasyonlar ve yükleme durumları için CSS stilleri ekle
const style = document.createElement('style');
style.textContent = `
    /* Yukarıdan aşağıya kayma animasyonu */
    .animate-in {
        animation: slideInUp 0.6s ease-out;
    }
    
    @keyframes slideInUp {
        from {
            opacity: 0;
            transform: translateY(30px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
    
    /* Yükleme durumu stili */
    .loading {
        position: relative;
        pointer-events: none; /* Tıklanamaz yap */
    }
    
    /* Yükleme spinner'ı */
    .loading::after {
        content: '';
        position: absolute;
        top: 50%;
        left: 50%;
        width: 20px;
        height: 20px;
        margin: -10px 0 0 -10px;
        border: 2px solid #f3f3f3;
        border-top: 2px solid #00bcd4;
        border-radius: 50%;
        animation: spin 1s linear infinite;
    }
    
    /* Spinner dönme animasyonu */
    @keyframes spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
    }
    
    /* Tooltip (ipucu baloncuğu) stili */
    .tooltip {
        position: absolute;
        background: rgba(0, 0, 0, 0.8);
        color: white;
        padding: 8px 12px;
        border-radius: 4px;
        font-size: 12px;
        white-space: nowrap;
        z-index: 1000;
        pointer-events: none; /* Tıklanamaz */
    }
    
    /* Hata durumu stili */
    .error {
        border-color: #f44336 !important;
        box-shadow: 0 0 0 2px rgba(244, 67, 54, 0.2) !important;
    }
    
    /* Focus durumu stili */
    .focused {
        transform: translateY(-2px); /* Hafif yukarı kaydır */
    }
`;
document.head.appendChild(style); // Stilleri sayfaya ekle