#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 64 // OLED display height, in pixels

// Declaration for an SSD1306 display connected to I2C (SDA, SCL pins)
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, -1);

const int led =  16;
const int buttonPin1 = 17;
const int buttonPin2 = 21;

//send serial stuff 
unsigned long lastSend = 0; 
const unsigned long SEND_INTERVAL = 20; //this is so it sends serial at abt 50Hz

//LED state 
bool ledBlinking = false;
bool ledState = false;
unsigned long lastBlink = 0;
const unsigned long BLINK_INTERVAL = 1000; 

void setup() {
  Serial.begin(115200);

  //LED digi pin
  pinMode(led, OUTPUT);

  //button digi pins 
  pinMode(buttonPin1, INPUT_PULLUP); //this makes it so buttons are false when being pressed, not all board support INPUT_PULLDOWN, so im just gonna invert it before we send.
  pinMode(buttonPin2, INPUT_PULLUP);

//cutting OLED out for now
  //   if(!display.begin(SSD1306_SWITCHCAPVCC, 0x3C)) { // Address 0x3D for 128x64
  //     Serial.println(F("SSD1306 allocation failed"));
  //     for(;;);
  //   }
  //   delay(2000);
  //   display.clearDisplay();
  //   display.setTextSize(1);
  //   display.setTextColor(WHITE);
  //   display.setCursor(0, 10);
  //   // Display static text
  //   display.println("Hello, world!");
  //   display.display();
  // }
}

void loop() {
  unsigned long now = millis(); //get current uptime

  //if its been long enough, send new serial values 
  if (now - lastSend >= SEND_INTERVAL){ 
    lastSend = now; //cache the new sendtime
    sendSerialValues();
  }
  receiveSerialValues(); //always check every loop if we received input from Unity
  blinkLight(now); //this is here but will bounce back out if the ledBlinking state flag isnt true 
}

void blinkLight(unsigned long now) {
  if (!ledBlinking) return; //state flag check

  if (now - lastBlink >= BLINK_INTERVAL){ //if its been long enough since hte last blink, do it again
    lastBlink = now;
    ledState = !ledState; //switch the state flag to the opposite (will cause the blinking)
    if (ledState){
      digitalWrite(led, HIGH); //drive digi pin high/low according to flag 
    }
    else digitalWrite(led, LOW);
  }            
}

void receiveSerialValues(void){
  if (!Serial.available()) return; 

  String command = Serial.readStringUntil('\n');
  command.trim();

  if (command == "LED_TEST!") {
    ledBlinking = true;
    lastBlink = millis();
  }
  else if (command == "LED_STOP!") {
    ledBlinking = false;
    ledState = false;
    digitalWrite(led, LOW);
  }
}

void sendSerialValues(void) {
  // reading buttons and knobs
  int joystickX = analogRead(A0);
  int joystickY = analogRead(A1);
  int slider = analogRead(A2);
  int pot = analogRead(A3);
  int button1State = digitalRead(buttonPin1);
  int button2State = digitalRead(buttonPin2);

  //Testing 
  Serial.print(joystickX);  Serial.print(",");
  Serial.print(joystickY);  Serial.print(",");
  Serial.print(slider);     Serial.print(",");
  Serial.print(pot);        Serial.print(",");
  Serial.print(!button1State);    Serial.print(",");
  Serial.println(!button2State);    //Serial.println sends the \n delimeter that Ardity needs, this is KEY for the end of a sent string 
}








