#define LED_PIN 7
#define PIR_PIN 13

int calibrationTime = 30;
long unsigned int lowIn;
long unsigned int pause = 100;
boolean lockLow = true;
boolean takeLowTime;
int PIRValue = 0;

void setup()
{
  pinMode(LED_PIN, OUTPUT);
  pinMode(PIR_PIN, INPUT);
  Serial.begin(9600);
}

void loop()
{
  PIRSensor();
}

void PIRSensor()
{
  if (digitalRead(PIR_PIN) == HIGH)
  {
    if (lockLow)
    {
      PIRValue = 1;
      lockLow = false;
      Serial.println("Motion detected.");
      digitalWrite(LED_PIN, HIGH);
      delay(50);
    }
    takeLowTime = true;
  }
  if (digitalRead(PIR_PIN) == LOW)
  {
    if (takeLowTime)
    {
      lowIn = millis();
      takeLowTime = false;
    }
    if (!lockLow && millis() - lowIn > pause)
    {
      PIRValue = 0;
      lockLow = true;
      Serial.println("Motion ended.");
      digitalWrite(LED_PIN, LOW);
      delay(50);
    }
  }
}
