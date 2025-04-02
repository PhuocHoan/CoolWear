# Test Cases

| Test Case | Steps                | Input                | Expected Result                      |
|-----------|----------------------|----------------------|--------------------------------------|
| TC001     | 1. Open application<br>2. Submit empty data | (No data)          | Display error for missing data       |
| TC002     | 1. Provide invalid data format<br>2. Submit | "abc123"           | Display error for invalid format     |
| TC003     | 1. Provide out-of-range data<br>2. Submit   | 999999            | Reject data as out of valid range    |
| TC004     | 1. Enter valid data<br>2. Submit            | Valid numeric only | Successful submission response       |
