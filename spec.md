# Hotel Stay Aggregator

## Features

### Search Hotels

Users can search hotels using:

- Destination
- Check-In Date
- Check-Out Date
- Room Type

### Reserve Hotel

Users can reserve a room.

## Reservation Process
the user shall be navigated to a Reservation
Form.
from the search results,
After selecting a hotel
The Reservation Form shall capture:
Guest Name Document Type Passport National ID Document Number
Business Rules:
International destinations require Passport. Domestic destinations allow National ID. Invalid documents return validation errors.
Upon successful reservation:
Generate Reservation Reference Number
Display Confirmation Page

### View Reservation

Users can retrieve reservation information using a reference number.

## Business Rules

- International destinations require Passport.
- Domestic destinations allow National ID.
- Invalid dates return Bad Request.
