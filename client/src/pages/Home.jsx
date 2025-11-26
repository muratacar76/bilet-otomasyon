import { useNavigate } from 'react-router-dom'

function Home() {
  const navigate = useNavigate()

  return (
    <div>
      <div className="hero">
        <h1>✈️ Hayalinizdeki Yolculuğa Başlayın</h1>
        <p>🌍 En uygun fiyatlarla uçak biletinizi hemen rezerve edin</p>
        <button className="btn btn-primary" onClick={() => navigate('/flights')} style={{ fontSize: '18px', padding: '16px 48px' }}>
          🔍 Uçuş Ara
        </button>
      </div>

      <div className="container">
        <div className="grid">
          <div className="card" style={{ 
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', 
            color: 'white',
            transform: 'translateY(0)',
            animation: 'slideInLeft 0.6s ease-out'
          }}>
            <h2 style={{ fontSize: '32px', marginBottom: '16px' }}>🎫 Üye Olmadan Rezervasyon</h2>
            <p style={{ fontSize: '16px', lineHeight: '1.6' }}>
              ✨ Sadece e-posta ile hızlı rezervasyon yapın!<br/>
              💳 Ödemenizi tamamlayın<br/>
              🎉 İsterseniz sonra üye olun
            </p>
          </div>
          <div className="card" style={{ 
            background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)', 
            color: 'white',
            animation: 'slideInLeft 0.8s ease-out'
          }}>
            <h2 style={{ fontSize: '32px', marginBottom: '16px' }}>💳 Güvenli Ödeme</h2>
            <p style={{ fontSize: '16px', lineHeight: '1.6' }}>Tüm ödemeleriniz güvenli altyapımız ile korunmaktadır. SSL şifreleme ile %100 güvenlik.</p>
          </div>
          <div className="card" style={{ 
            background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)', 
            color: 'white',
            animation: 'slideInLeft 1s ease-out'
          }}>
            <h2 style={{ fontSize: '32px', marginBottom: '16px' }}>🔄 Esnek İptal</h2>
            <p style={{ fontSize: '16px', lineHeight: '1.6' }}>Uçuştan 24 saat öncesine kadar ücretsiz iptal ve değişiklik yapabilirsiniz.</p>
          </div>
        </div>

        <div style={{ 
          marginTop: '60px', 
          textAlign: 'center',
          background: 'linear-gradient(135deg, rgba(255,255,255,0.95) 0%, rgba(248,249,255,0.95) 100%)',
          padding: '40px',
          borderRadius: '30px',
          boxShadow: '0 15px 50px rgba(0,0,0,0.2)',
          animation: 'fadeIn 1.2s ease-in'
        }}>
          <h2 style={{ 
            fontSize: '36px', 
            marginBottom: '24px',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            fontWeight: '900'
          }}>
            🌟 Neden Bizi Seçmelisiniz?
          </h2>
          <div className="grid" style={{ marginTop: '30px' }}>
            <div style={{ padding: '20px' }}>
              <div style={{ fontSize: '48px', marginBottom: '12px' }}>⚡</div>
              <h3 style={{ color: '#667eea', marginBottom: '8px' }}>Hızlı İşlem</h3>
              <p style={{ color: '#666' }}>Dakikalar içinde rezervasyon</p>
            </div>
            <div style={{ padding: '20px' }}>
              <div style={{ fontSize: '48px', marginBottom: '12px' }}>💰</div>
              <h3 style={{ color: '#f093fb', marginBottom: '8px' }}>En İyi Fiyat</h3>
              <p style={{ color: '#666' }}>Garantili düşük fiyatlar</p>
            </div>
            <div style={{ padding: '20px' }}>
              <div style={{ fontSize: '48px', marginBottom: '12px' }}>🎯</div>
              <h3 style={{ color: '#4facfe', marginBottom: '8px' }}>7/24 Destek</h3>
              <p style={{ color: '#666' }}>Her zaman yanınızdayız</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Home
