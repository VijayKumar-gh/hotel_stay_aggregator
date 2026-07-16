import { FormEvent, useState } from 'react';

type HotelOffer = {
  hotelId: string;
  hotelName: string;
  destination: string;
  roomType: string;
  pricePerNight: number;
  currency: string;
  providerName: string;
  isAvailable: boolean;
};

type Reservation = {
  referenceNumber: string;
  hotelName: string;
  destination: string;
  checkInDate: string;
  checkOutDate: string;
  guestName: string;
  providerName: string;
  totalPrice: number;
  currency: string;
};

type ReservationFormState = {
  guestName: string;
  guestDocumentType: string;
  guestDocumentNumber: string;
};

type FlowStep = 'search' | 'reservation' | 'confirmation' | 'lookup';

const defaultSearch = {
  destination: 'Berlin',
  checkInDate: '2026-08-01',
  checkOutDate: '2026-08-03',
  roomType: 'Deluxe',
};

const defaultReservationForm = {
  guestName: 'Ada Lovelace',
  guestDocumentType: 'Passport',
  guestDocumentNumber: 'P1234567',
};

const isDomesticDestination = (destination: string) => {
  const normalized = destination.trim().toLowerCase();
  return ['new york', 'seattle', 'boston', 'austin', 'miami'].includes(normalized);
};

function App() {
  const [search, setSearch] = useState(defaultSearch);
  const [results, setResults] = useState<HotelOffer[]>([]);
  const [selectedOffer, setSelectedOffer] = useState<HotelOffer | null>(null);
  const [reservationForm, setReservationForm] = useState<ReservationFormState>(defaultReservationForm);
  const [confirmation, setConfirmation] = useState<Reservation | null>(null);
  const [reservationLookup, setReservationLookup] = useState<Reservation | null>(null);
  const [referenceNumber, setReferenceNumber] = useState('');
  const [error, setError] = useState('');
  const [searchErrors, setSearchErrors] = useState<{ destination?: string; checkInDate?: string; checkOutDate?: string; roomType?: string }>({});
  const [reservationErrors, setReservationErrors] = useState<{ guestName?: string; guestDocumentType?: string; guestDocumentNumber?: string }>({});
  const [activeStep, setActiveStep] = useState<FlowStep>('search');

  const canAccessReservationStep = Boolean(selectedOffer);
  const canAccessConfirmationStep = Boolean(confirmation);
  const canAccessLookupStep = Boolean(referenceNumber.trim());

  const searchHotels = async (event: FormEvent) => {
    event.preventDefault();
    setError('');
    setSelectedOffer(null);
    setConfirmation(null);
    setReservationLookup(null);
    setActiveStep('search');

    const nextErrors: { destination?: string; checkInDate?: string; checkOutDate?: string; roomType?: string } = {};

    if (!search.destination.trim()) {
      nextErrors.destination = 'Destination is required.';
    }

    if (!search.checkInDate) {
      nextErrors.checkInDate = 'Check-in date is required.';
    }

    if (!search.checkOutDate) {
      nextErrors.checkOutDate = 'Check-out date is required.';
    }

    if (!search.roomType.trim()) {
      nextErrors.roomType = 'Room type is required.';
    }

    if (search.checkInDate && search.checkOutDate && new Date(search.checkOutDate) <= new Date(search.checkInDate)) {
      nextErrors.checkOutDate = 'Check-out date must be after check-in date.';
    }

    setSearchErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) {
      return;
    }

    const response = await fetch(
      `http://localhost:5042/api/hotels/search?destination=${encodeURIComponent(search.destination)}&checkInDate=${encodeURIComponent(search.checkInDate)}&checkOutDate=${encodeURIComponent(search.checkOutDate)}&roomType=${encodeURIComponent(search.roomType)}`,
    );

    if (!response.ok) {
      setError('Search failed.');
      return;
    }

    const data = (await response.json()) as HotelOffer[];
    setResults(data);
  };

  const startReservation = (offer: HotelOffer) => {
    setSelectedOffer(offer);
    setError('');
    setConfirmation(null);
    setActiveStep('reservation');
  };

  const submitReservation = async (event: FormEvent) => {
    event.preventDefault();

    if (!selectedOffer) {
      setError('Please select a hotel first.');
      return;
    }

    const nextErrors: { guestName?: string; guestDocumentType?: string; guestDocumentNumber?: string } = {};

    if (!reservationForm.guestName.trim()) {
      nextErrors.guestName = 'Guest name is required.';
    }

    if (!reservationForm.guestDocumentNumber.trim()) {
      nextErrors.guestDocumentNumber = 'Document number is required.';
    }

    const destinationIsDomestic = isDomesticDestination(selectedOffer.destination);
    const requiredDocumentType = destinationIsDomestic ? 'National ID' : 'Passport';

    if (reservationForm.guestDocumentType !== requiredDocumentType) {
      nextErrors.guestDocumentType = destinationIsDomestic
        ? 'Domestic destinations require National ID.'
        : 'International destinations require Passport.';
    }

    setReservationErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) {
      return;
    }

    const response = await fetch('http://localhost:5042/api/reservations', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        hotelId: selectedOffer.hotelId,
        hotelName: selectedOffer.hotelName,
        destination: selectedOffer.destination,
        checkInDate: search.checkInDate,
        checkOutDate: search.checkOutDate,
        roomType: selectedOffer.roomType,
        guestName: reservationForm.guestName,
        guestDocumentType: reservationForm.guestDocumentType,
        guestDocumentNumber: reservationForm.guestDocumentNumber,
        providerName: selectedOffer.providerName,
      }),
    });

    if (!response.ok) {
      const message = await response.text();
      setError(message || 'Reservation failed.');
      return;
    }

    const payload = (await response.json()) as Reservation & { referenceNumber: string };
    setConfirmation(payload);
    setSelectedOffer(null);
    setReferenceNumber(payload.referenceNumber);
    setError('');
    setActiveStep('confirmation');
  };

  const viewReservation = async () => {
    if (!referenceNumber.trim()) {
      setError('Reference number is required.');
      return;
    }

    const response = await fetch(`http://localhost:5042/api/reservations/${referenceNumber}`);
    if (!response.ok) {
      setError('Reservation not found.');
      return;
    }

    const data = (await response.json()) as Reservation;
    setReservationLookup(data);
    setError('');
    setActiveStep('lookup');
  };

  return (
    <div className="app-shell">
      <h1>Hotel Stay Aggregator</h1>
      <p>Compare offers from PremierStays and BudgetNests.</p>

      <div className="stepper">
        <button type="button" className={activeStep === 'search' ? 'step active' : 'step'} onClick={() => setActiveStep('search')}>1. Search</button>
        <button type="button" className={activeStep === 'reservation' ? 'step active' : 'step'} onClick={() => canAccessReservationStep && setActiveStep('reservation')} disabled={!canAccessReservationStep}>2. Reservation</button>
        <button type="button" className={activeStep === 'confirmation' ? 'step active' : 'step'} onClick={() => canAccessConfirmationStep && setActiveStep('confirmation')} disabled={!canAccessConfirmationStep}>3. Confirmation</button>
        <button type="button" className={activeStep === 'lookup' ? 'step active' : 'step'} onClick={() => canAccessLookupStep && setActiveStep('lookup')} disabled={!canAccessLookupStep}>4. Lookup</button>
      </div>

      {activeStep === 'search' ? (
        <form onSubmit={searchHotels} className="search-form">
          <div>
            <input value={search.destination} onChange={(e) => setSearch({ ...search, destination: e.target.value })} placeholder="Destination" />
            {searchErrors.destination ? <div className="field-error">{searchErrors.destination}</div> : null}
          </div>
          <div>
            <input type="date" value={search.checkInDate} onChange={(e) => setSearch({ ...search, checkInDate: e.target.value })} />
            {searchErrors.checkInDate ? <div className="field-error">{searchErrors.checkInDate}</div> : null}
          </div>
          <div>
            <input type="date" value={search.checkOutDate} onChange={(e) => setSearch({ ...search, checkOutDate: e.target.value })} />
            {searchErrors.checkOutDate ? <div className="field-error">{searchErrors.checkOutDate}</div> : null}
          </div>
          <div>
            <input value={search.roomType} onChange={(e) => setSearch({ ...search, roomType: e.target.value })} placeholder="Room type" />
            {searchErrors.roomType ? <div className="field-error">{searchErrors.roomType}</div> : null}
          </div>
          <button type="submit">Search Hotels</button>
        </form>
      ) : null}

      {error ? <div className="error">{error}</div> : null}

      {activeStep === 'search' ? (
        <section>
          <h2>Search Results</h2>
          <ul className="hotel-list">
            {results.map((offer) => (
              <li key={`${offer.providerName}-${offer.hotelId}`}>
                <strong>{offer.hotelName}</strong> ({offer.providerName})
                <div>{offer.destination} • {offer.roomType}</div>
                <div>{offer.pricePerNight} {offer.currency}/night</div>
                <button onClick={() => startReservation(offer)}>Select Hotel</button>
              </li>
            ))}
          </ul>
        </section>
      ) : null}

      {selectedOffer && activeStep === 'reservation' ? (
        <section>
          <h2>Reservation Form</h2>
          <form onSubmit={submitReservation} className="reservation-form">
            <div>
              <strong>Selected Hotel:</strong> {selectedOffer.hotelName} ({selectedOffer.providerName})
            </div>
            <div>
              <input
                value={reservationForm.guestName}
                onChange={(e) => setReservationForm({ ...reservationForm, guestName: e.target.value })}
                placeholder="Guest Name"
              />
              {reservationErrors.guestName ? <div className="field-error">{reservationErrors.guestName}</div> : null}
            </div>
            <div>
              <select
                value={reservationForm.guestDocumentType}
                onChange={(e) => setReservationForm({ ...reservationForm, guestDocumentType: e.target.value })}
              >
                <option value="Passport">Passport</option>
                <option value="National ID">National ID</option>
              </select>
              {reservationErrors.guestDocumentType ? <div className="field-error">{reservationErrors.guestDocumentType}</div> : null}
            </div>
            <div>
              <input
                value={reservationForm.guestDocumentNumber}
                onChange={(e) => setReservationForm({ ...reservationForm, guestDocumentNumber: e.target.value })}
                placeholder="Document Number"
              />
              {reservationErrors.guestDocumentNumber ? <div className="field-error">{reservationErrors.guestDocumentNumber}</div> : null}
            </div>
            <button type="submit">Confirm Reservation</button>
          </form>
        </section>
      ) : null}

      {confirmation && activeStep === 'confirmation' ? (
        <section>
          <h2>Confirmation Page</h2>
          <div className="reservation-card">
            <div><strong>Reference Number:</strong> {confirmation.referenceNumber}</div>
            <div><strong>Hotel:</strong> {confirmation.hotelName}</div>
            <div><strong>Destination:</strong> {confirmation.destination}</div>
            <div><strong>Guest:</strong> {confirmation.guestName}</div>
            <div><strong>Total:</strong> {confirmation.totalPrice} {confirmation.currency}</div>
          </div>
        </section>
      ) : null}

      {activeStep === 'lookup' ? (
        <section>
          <h2>View Reservation</h2>
          <div className="reservation-form">
            <input value={referenceNumber} onChange={(e) => setReferenceNumber(e.target.value)} placeholder="Reference number" />
            <button onClick={viewReservation}>Lookup</button>
          </div>
          {reservationLookup ? (
            <div className="reservation-card">
              <div>Reference: {reservationLookup.referenceNumber}</div>
              <div>Hotel: {reservationLookup.hotelName}</div>
              <div>Destination: {reservationLookup.destination}</div>
              <div>Guest: {reservationLookup.guestName}</div>
              <div>Total: {reservationLookup.totalPrice} {reservationLookup.currency}</div>
            </div>
          ) : null}
        </section>
      ) : null}
    </div>
  );
}

export default App;
