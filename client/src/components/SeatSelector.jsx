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
    onSeatsSelected(newSelectedSeats);
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
        <h3>Koltuk Seçimi</h3>
        <p>Lütfen {passengerCount} koltuk seçin ({selectedSeats.length}/{passengerCount})</p>
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

      {selectedSeats.length > 0 && (
        <div className="selected-seats-summary">
          <h4>Seçilen Koltuklar:</h4>
          <div className="selected-seats-list">
            {selectedSeats.map((seat, index) => (
              <span key={seat.seatNumber} className="selected-seat-badge">
                {seat.seatNumber} ({seat.seatType === 'Window' ? '🪟 Cam' : seat.seatType === 'Aisle' ? '🚶 Koridor' : '💺 Orta'})
              </span>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default SeatSelector;
