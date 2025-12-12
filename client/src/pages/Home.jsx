import { useNavigate } from 'react-router-dom'

function Home() {
  const navigate = useNavigate()

  return (
    <div>
      <div className="hero">
        <h1>BULUTBİLET<span style={{ color: '#00e5ff' }}>.COM</span></h1>
        <h2 style={{ fontSize: '28px', marginTop: '16px', marginBottom: '16px' }}>Hayalinizdeki Yolculuğa Başlayın</h2>
        <p>🌍 En uygun fiyatlarla uçak biletinizi hemen rezerve edin</p>
        <button className="btn btn-primary" onClick={() => navigate('/flights')} style={{ fontSize: '18px', padding: '16px 48px' }}>
          🔍 Uçuş Ara
        </button>
      </div>

      <div className="container">
        <div className="grid">
          <div className="card" style={{ 
            background: 'linear-gradient(135deg, #00bcd4 0%, #00acc1 100%)', 
            color: 'white',
            transform: 'translateY(0)',
            animation: 'slideInLeft 0.6s ease-out',
            border: 'none'
          }}>
            <h2 style={{ fontSize: '28px', marginBottom: '20px', fontWeight: '700' }}>🎫 Üye Olmadan Rezervasyon</h2>
            <p style={{ fontSize: '16px', lineHeight: '1.7', fontWeight: '400' }}>
              ✨ Sadece e-posta ile hızlı rezervasyon yapın!<br/>
              💳 Ödemenizi güvenle tamamlayın<br/>
              🎉 İsterseniz sonra üye olun
            </p>
          </div>
          <div className="card" style={{ 
            background: 'linear-gradient(135deg, #0097a7 0%, #00838f 100%)', 
            color: 'white',
            animation: 'slideInLeft 0.8s ease-out',
            border: 'none'
          }}>
            <h2 style={{ fontSize: '28px', marginBottom: '20px', fontWeight: '700' }}>💳 Güvenli Ödeme</h2>
            <p style={{ fontSize: '16px', lineHeight: '1.7', fontWeight: '400' }}>Tüm ödemeleriniz güvenli altyapımız ile korunmaktadır. SSL şifreleme ile %100 güvenlik garantisi.</p>
          </div>
          <div className="card" style={{ 
            background: 'linear-gradient(135deg, #006064 0%, #004d40 100%)', 
            color: 'white',
            animation: 'slideInLeft 1s ease-out',
            border: 'none'
          }}>
            <h2 style={{ fontSize: '28px', marginBottom: '20px', fontWeight: '700' }}>🔄 Esnek İptal</h2>
            <p style={{ fontSize: '16px', lineHeight: '1.7', fontWeight: '400' }}>Uçuştan 24 saat öncesine kadar ücretsiz iptal ve değişiklik imkanı sunuyoruz.</p>
          </div>
        </div>

        <div style={{ 
          marginTop: '80px', 
          textAlign: 'center',
          background: 'linear-gradient(135deg, rgba(255,255,255,0.95) 0%, rgba(224,247,250,0.95) 100%)',
          backdropFilter: 'blur(20px)',
          padding: '60px 40px',
          borderRadius: '32px',
          boxShadow: '0 20px 80px rgba(0,188,212,0.15)',
          animation: 'fadeIn 1.2s ease-in',
          border: '1px solid rgba(0,188,212,0.1)'
        }}>
          <h2 style={{ 
            fontSize: '42px', 
            marginBottom: '32px',
            background: 'linear-gradient(135deg, #00bcd4 0%, #00acc1 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            fontWeight: '800',
            fontFamily: 'Inter, sans-serif'
          }}>
            🌟 Neden Bizi Seçmelisiniz?
          </h2>
          <div className="grid" style={{ marginTop: '40px' }}>
            <div style={{ padding: '24px' }}>
              <div style={{ fontSize: '56px', marginBottom: '16px' }}>⚡</div>
              <h3 style={{ color: '#00bcd4', marginBottom: '12px', fontSize: '20px', fontWeight: '600' }}>Hızlı İşlem</h3>
              <p style={{ color: '#546e7a', fontSize: '16px', lineHeight: '1.6' }}>Dakikalar içinde rezervasyon</p>
            </div>
            <div style={{ padding: '24px' }}>
              <div style={{ fontSize: '56px', marginBottom: '16px' }}>💰</div>
              <h3 style={{ color: '#0097a7', marginBottom: '12px', fontSize: '20px', fontWeight: '600' }}>En İyi Fiyat</h3>
              <p style={{ color: '#546e7a', fontSize: '16px', lineHeight: '1.6' }}>Garantili düşük fiyatlar</p>
            </div>
            <div style={{ padding: '24px' }}>
              <div style={{ fontSize: '56px', marginBottom: '16px' }}>🎯</div>
              <h3 style={{ color: '#00838f', marginBottom: '12px', fontSize: '20px', fontWeight: '600' }}>7/24 Destek</h3>
              <p style={{ color: '#546e7a', fontSize: '16px', lineHeight: '1.6' }}>Her zaman yanınızdayız</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Home
