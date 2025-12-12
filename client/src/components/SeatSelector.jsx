import { useState, useEffect } from 'react';
import axios from 'axios';

const SeatSelector = ({ flightId, passengerCount, onSeatsSelected }) => {
  const [seatMap, setSeatMap] = useState(null);
  const [selectedSeats, setSelectedSeats] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchSeatMap();
  }, [flightId]);

  const fetchSeatMap = async () => {
    try {
      const response = await axios.get(`/api/flights/${flightId}/seats`);
      setSeatMap(response.data);
      setLoading(false);
    } catch (error) {
      console.error('Koltuk haritası yüklenemedi:', error);
      setLoading(false);
    }
  };

  const handleSeatClick = (seat) => {
    if (seat.isOccupied) return;

    const isSelected = selectedSeats.some(s => s.seatNumber === seat.seatNumber);
    
    let newSelectedSeats;
    if (isSelected) {
      newSelectedSeats = selectedSeats.filter(s => s.seatNumber !== seat.seatNumber);
    } else {
      if (selectedSeats.length >= passengerCount) {
        alert(`En fazla ${passengerCount} koltuk seçebilirsiniz`);
        return;
      }
      newSelectedSeats = [...selectedSeats, {
        seatNumber: seat.seatNumber,
        seatType: seat.seatType
      }];
    }
    
    setSelectedSeats(newSelectedSeats);
  };

  const getSeatClass = (seat) => {
    if (seat.isOccupied) return 'seat occupied';
    if (selectedSeats.some(s => s.seatNumber === seat.seatNumber)) return 'seat selected';
    if (seat.seatType === 'Window') return 'seat window';
    if (seat.seatType === 'Aisle') return 'seat aisle';
    return 'seat middle';
  };

  const getSeatIcon = (seatType) => {
    if (seatType === 'Window') return '🪟';
    if (seatType === 'Aisle') return '🚶';
    return '💺';
  };

  const isExitRow = (rowNumber) => {
    // Çıkış kapısı sıraları: 1, 10, 20, 30 (ön, kanat üstü, arka)
    return rowNumber === 1 || rowNumber === 10 || rowNumber === 20 || rowNumber === 30;
  };

  if (loading) {
    return <div className="seat-selector-loading">Koltuk haritası yükleniyor...</div>;
  }

  if (!seatMap) {
    return <div className="seat-selector-error">Koltuk haritası yüklenemedi</div>;
  }

  return (
    <div className="seat-selector">
      <div className="seat-selector-header">
        <h3>Koltuk Seçimi - {passengerCount} Yolcu</h3>
        <div style={{
          background: selectedSeats.length === passengerCount ? 'linear-gradient(135deg, #4caf50, #388e3c)' : 'linear-gradient(135deg, #00bcd4, #00acc1)',
          color: 'white',
          padding: '16px 20px',
          borderRadius: '12px',
          textAlign: 'center',
          marginBottom: '20px'
        }}>
          {selectedSeats.length === passengerCount 
            ? `✅ Tüm yolcular için koltuk seçildi! (${passengerCount}/${passengerCount})`
            : selectedSeats.length === 0
              ? `🪑 ${passengerCount} yolcu için koltuk seçin`
              : `🪑 ${selectedSeats.length}/${passengerCount} koltuk seçildi - ${passengerCount - selectedSeats.length} koltuk daha seçin`
          }
        </div>
        {selectedSeats.length > 0 && selectedSeats.length < passengerCount && (
          <div style={{
            background: '#fff3cd',
            border: '1px solid #ffeaa7',
            color: '#856404',
            padding: '8px 12px',
            borderRadius: '6px',
            fontSize: '14px',
            marginBottom: '12px'
          }}>
            💡 Şu anda <strong>Yolcu {selectedSeats.length + 1}</strong> için koltuk seçiyorsunuz
          </div>
        )}
      </div>

      <div className="seat-legend">
        <div className="legend-item">
          <div className="seat available"></div>
          <span>Müsait</span>
        </div>
        <div className="legend-item">
          <div className="seat window"></div>
          <span>🪟 Cam Kenarı</span>
        </div>
        <div className="legend-item">
          <div className="seat aisle"></div>
          <span>🚶 Koridor</span>
        </div>
        <div className="legend-item">
          <div className="seat occupied"></div>
          <span>Dolu</span>
        </div>
        <div className="legend-item">
          <div className="seat selected"></div>
          <span>Seçili</span>
        </div>
        <div className="legend-item">
          <div className="exit-door" style={{ fontSize: '11px', padding: '4px 8px' }}>🚪</div>
          <span>Çıkış Sırası</span>
        </div>
      </div>

      <div className="airplane-cabin">
        <div className="cabin-header">
          <div className="cockpit">✈️ Kokpit</div>
        </div>
        
        <div className="seat-map">
          {seatMap.seatLayout.map((row) => (
            <div key={row.row}>
              {isExitRow(row.row) && (
                <div className="exit-row-marker">
                  <div className="exit-door left">🚪 Çıkış Kapısı</div>
                  <div className="exit-door right">Çıkış Kapısı 🚪</div>
                </div>
              )}
              <div className={`seat-row ${isExitRow(row.row) ? 'exit-row' : ''}`}>
                <div className="row-number">{row.row}</div>
                <div className="seats-container">
                  {row.seats.map((seat, index) => (
                    <div key={seat.seatNumber}>
                      <button
                        className={getSeatClass(seat)}
                        onClick={() => handleSeatClick(seat)}
                        disabled={seat.isOccupied}
                        title={`${seat.seatNumber} - ${seat.seatType === 'Window' ? 'Cam Kenarı' : seat.seatType === 'Aisle' ? 'Koridor' : 'Orta'}${isExitRow(row.row) ? ' - Çıkış Sırası (Ekstra Bacak Mesafesi)' : ''}`}
                      >
                        <span className="seat-icon">{getSeatIcon(seat.seatType)}</span>
                        <span className="seat-label">{seat.column}</span>
                      </button>
                      {index === 2 && <div className="aisle-space"></div>}
                    </div>
                  ))}
                </div>
                <div className="row-number">{row.row}</div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="selected-seats-summary">
        {selectedSeats.length > 0 && (
          <>
            <h4>Seçilen Koltuklar:</h4>
            <div className="selected-seats-list">
              {selectedSeats.map((seat, index) => (
                <div key={seat.seatNumber} className="selected-seat-item" style={{
                  display: 'inline-block',
                  margin: '6px',
                  padding: '10px 16px',
                  background: 'linear-gradient(135deg, #00bcd4, #00acc1)',
                  color: 'white',
                  borderRadius: '12px',
                  fontSize: '15px',
                  fontWeight: '500'
                }}>
                  <strong>Yolcu {index + 1}:</strong> {seat.seatNumber} ({seat.seatType === 'Window' ? '🪟 Cam' : seat.seatType === 'Aisle' ? '🚶 Koridor' : '💺 Orta'})
                </div>
              ))}
            </div>
          </>
        )}
        
        <button 
          className={`btn ${selectedSeats.length === passengerCount ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => {
            if (selectedSeats.length !== passengerCount) {
              alert(`⚠️ Lütfen ${passengerCount} koltuk seçin. Şu anda ${selectedSeats.length} koltuk seçili.`)
              return
            }
            onSeatsSelected(selectedSeats)
          }}
          disabled={selectedSeats.length !== passengerCount}
          style={{ 
            marginTop: '16px', 
            width: '100%',
            opacity: selectedSeats.length === passengerCount ? 1 : 0.6,
            cursor: selectedSeats.length === passengerCount ? 'pointer' : 'not-allowed'
          }}
        >
          {selectedSeats.length === passengerCount 
            ? `✅ Koltuk Seçimini Onayla (${selectedSeats.length} Koltuk)` 
            : `🚫 Koltuk Seçin (${selectedSeats.length}/${passengerCount}) - ${passengerCount - selectedSeats.length} eksik`
          }
        </button>
        
        {selectedSeats.length > 0 && selectedSeats.length !== passengerCount && (
          <p style={{ 
            textAlign: 'center', 
            marginTop: '8px', 
            color: '#666', 
            fontSize: '14px' 
          }}>
            {passengerCount - selectedSeats.length} koltuk daha seçmeniz gerekiyor
          </p>
        )}
      </div>
    </div>
  );
};

export default SeatSelector;
