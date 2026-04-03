#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 64

Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, -1);

// ─── PIN DEFINITIONS ──────────────────────────────────────────────
// Player sliders (analog)
const int PIN_playerPot_a    = A9;
const int PIN_playerSlider_h = A0;
const int PIN_playerPot_k    = A10;
const int PIN_playerSlider_c = A1;

// Sub control panel (digital inputs)
const int PIN_oxyL1    = 45;
const int PIN_oxyL2    = 46;
const int PIN_oxyL3    = 47;
const int PIN_ping     = 44;
const int PIN_radarOn  = 41;
const int PIN_radarOff = 40;
const int PIN_door     = 43;

// Coolant (analog + digital outputs)
const int PIN_coolantPot   = A11;
const int PIN_coolantServo = 12;  // OUTPUT
const int PIN_tempServo    = 13;  // OUTPUT

// Lighting sliders (analog)
const int PIN_headSlider  = A2;
const int PIN_floodSlider = A3;

// Sub pilot (digital + analog)
const int PIN_shoot = 42;
const int PIN_joy1X = A5;
const int PIN_joy1Y = A6;
const int PIN_joy2X = A7;
const int PIN_joy2Y = A8;

// ─── TIMING ───────────────────────────────────────────────────────
unsigned long lastSend = 0;
const unsigned long SEND_INTERVAL = 250; // 20ms = ~50Hz

// ─── LED BLINK STATE ──────────────────────────────────────────────
bool ledBlinking = false;
bool ledState = false;
unsigned long lastBlink = 0;
const unsigned long BLINK_INTERVAL = 1000;

// ─────────────────────────────────────────────────────────────────
void setup() {
  Serial.begin(115200);

  // Digital inputs
  pinMode(PIN_oxyL1,    INPUT);
  pinMode(PIN_oxyL2,    INPUT);
  pinMode(PIN_oxyL3,    INPUT);
  pinMode(PIN_ping,     INPUT);
  pinMode(PIN_radarOn,  INPUT);
  pinMode(PIN_radarOff, INPUT);
  pinMode(PIN_door,     INPUT);
  pinMode(PIN_shoot,    INPUT);

  // Digital outputs
  pinMode(PIN_coolantServo, OUTPUT);
  pinMode(PIN_tempServo,    OUTPUT);

  // OLED (uncomment to enable)
  // if (!display.begin(SSD1306_SWITCHCAPVCC, 0x3C)) {
  //   Serial.println(F("SSD1306 allocation failed"));
  //   for (;;);
  // }
  // delay(2000);
  // display.clearDisplay();
  // display.setTextSize(1);
  // display.setTextColor(WHITE);
  // display.setCursor(0, 10);
  // display.println("Hello, world!");
  // display.display();
}

// ─────────────────────────────────────────────────────────────────
void loop() {
  unsigned long now = millis();

  if (now - lastSend >= SEND_INTERVAL) {
    lastSend = now;
    sendSerialValues();
  }

  receiveSerialValues();

  // blinkLight(now); // uncomment if using LED
}

// ─────────────────────────────────────────────────────────────────
void receiveSerialValues() {
  if (!Serial.available()) return;

  String command = Serial.readStringUntil('\n');
  command.trim();

  // if (command == "LED_TEST!") {
  //   ledBlinking = true;
  //   lastBlink = millis();
  // }
  // else if (command == "LED_STOP!") {
  //   ledBlinking = false;
  //   ledState = false;
  //   digitalWrite(ledPin, LOW);
  // }
}

// ─────────────────────────────────────────────────────────────────
void sendSerialValues() {
  // Player sliders
  Serial.print(analogRead(PIN_playerPot_a));    Serial.print(",");
  Serial.print(analogRead(PIN_playerSlider_h)); Serial.print(",");
  Serial.print(analogRead(PIN_playerPot_k));    Serial.print(",");
  Serial.print(analogRead(PIN_playerSlider_c)); Serial.print(",");
  
  // Sub control panel
  Serial.print(digitalRead(PIN_oxyL1));    Serial.print(",");
  Serial.print(digitalRead(PIN_oxyL2));    Serial.print(",");
  Serial.print(digitalRead(PIN_oxyL3));    Serial.print(",");
  Serial.print(digitalRead(PIN_ping));     Serial.print(",");
  Serial.print(digitalRead(PIN_radarOn));  Serial.print(",");
  Serial.print(digitalRead(PIN_radarOff)); Serial.print(",");
  Serial.print(digitalRead(PIN_door));     Serial.print(",");

  // Coolant
  Serial.print(analogRead(PIN_coolantPot)); Serial.print(",");

  // Lighting
  Serial.print(analogRead(PIN_headSlider));  Serial.print(",");
  Serial.print(analogRead(PIN_floodSlider)); Serial.print(",");

  // Pilot
  Serial.print(digitalRead(PIN_shoot)); Serial.print(",");
  Serial.print(analogRead(PIN_joy1X));  Serial.print(",");
  Serial.print(analogRead(PIN_joy1Y));  Serial.print(",");
  Serial.print(analogRead(PIN_joy2X));  Serial.print(",");
  Serial.println(analogRead(PIN_joy2Y)); // println adds the \n delimiter Ardity needs

  // ── Player A Controls ──────────────────────────────
  // Serial.print("playerPot_a:");    Serial.print(analogRead(PIN_playerPot_a));    Serial.print(",");
  // Serial.print("playerSlider_h:"); Serial.print(analogRead(PIN_playerSlider_h)); Serial.print(",");
  // Serial.print("playerPot_k:");    Serial.print(analogRead(PIN_playerPot_k));    Serial.print(",");
  // Serial.print("playerSlider_c:"); Serial.print(analogRead(PIN_playerSlider_c)); Serial.print(",");

  // // ── Oxygen Levels ──────────────────────────────────
   Serial.print("oxyL1:");    Serial.println(digitalRead(PIN_oxyL1));    Serial.print(",");
   Serial.print("oxyL2:");    Serial.println(digitalRead(PIN_oxyL2));    Serial.print(",");
   Serial.print("oxyL3:");    Serial.println(digitalRead(PIN_oxyL3));    Serial.print(",");

  // // ── Radar & Sonar ──────────────────────────────────
  // Serial.print("ping:");     Serial.print(digitalRead(PIN_ping));     Serial.print(",");
  // Serial.print("radarOn:");  Serial.print(digitalRead(PIN_radarOn));  Serial.print(",");
  // Serial.print("radarOff:"); Serial.print(digitalRead(PIN_radarOff)); Serial.print(",");

  // // ── Door ───────────────────────────────────────────
  // Serial.print("door:");     Serial.print(digitalRead(PIN_door));     Serial.print(",");

  // // ── Coolant ────────────────────────────────────────
  // Serial.print("coolantPot:"); Serial.print(analogRead(PIN_coolantPot)); Serial.print(",");

  // // ── Lighting ───────────────────────────────────────
  // Serial.print("headSlider:");  Serial.print(analogRead(PIN_headSlider));  Serial.print(",");
  // Serial.print("floodSlider:"); Serial.print(analogRead(PIN_floodSlider)); Serial.print(",");

  // // ── Pilot Controls ─────────────────────────────────
  // Serial.print("shoot:"); Serial.print(digitalRead(PIN_shoot)); Serial.print(",");
  // Serial.print("joy1X:"); Serial.print(analogRead(PIN_joy1X));  Serial.print(",");
  // Serial.print("joy1Y:"); Serial.print(analogRead(PIN_joy1Y));  Serial.print(",");
  // Serial.print("joy2X:"); Serial.print(analogRead(PIN_joy2X));  Serial.print(",");
  // Serial.print("joy2Y:"); Serial.println(analogRead(PIN_joy2Y)); // \n delimiter — keep at end
}

// ─────────────────────────────────────────────────────────────────
// void blinkLight(unsigned long now) {
//   if (!ledBlinking) return;
//   if (now - lastBlink >= BLINK_INTERVAL) {
//     lastBlink = now;
//     ledState = !ledState;
//     digitalWrite(ledPin, ledState ? HIGH : LOW);
//   }
// }
