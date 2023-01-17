#include <SerialDebug.h>
#include <FastLED.h>

// Select the timers you're using, here ITimer1
#define USE_TIMER_1 true

#include "TimerInterrupt.h"

#define LED_PIN 5
#define NUM_LEDS 60
#define BRIGHTNESS 64
#define LED_TYPE WS2811
#define COLOR_ORDER GRB
#define MOTION_ZONES 60

CRGB leds[NUM_LEDS];

#define TIMER_INTERVAL_MS 125L

int motions[MOTION_ZONES];
int zoneSize = NUM_LEDS / MOTION_ZONES;
uint8_t cIndex = 0;

CRGBPalette16 currentPalette;
TBlendType currentBlending;

void setup()
{
  delay(3000); // power-up safety delay
  FastLED.addLeds<LED_TYPE, LED_PIN, COLOR_ORDER>(leds, NUM_LEDS).setCorrection(TypicalLEDStrip);
  FastLED.setBrightness(BRIGHTNESS);

  currentPalette = RainbowColors_p;
  currentBlending = LINEARBLEND;

  SetupTimer();
}

// Timer callback to simulate walking up motion zones.
int timerCallbackCount = 0;
void TimerHandler()
{
  Serial.println(F("In timer callback."));
  // decay previous motion zone
  for (int i = 0; i < MOTION_ZONES; i++)
  {
    if (motions[i] > 0)
    {
      motions[i]--;
    }
  }

  int activeMotionZone = (timerCallbackCount++) % MOTION_ZONES;
  if (activeMotionZone >= MOTION_ZONES)
    cIndex += 3;
  motions[activeMotionZone] = 20;
}

void SetupTimer()
{
  cli(); // stop interrupts

  // Init timer ITimer1
  ITimer1.init();

  // Using ATmega328 used in UNO => 16MHz CPU clock ,
  // Interval in millisecs
  if (ITimer1.attachInterruptInterval(TIMER_INTERVAL_MS, TimerHandler))
  {
    Serial.print(F("Starting  ITimer1 OK, millis() = "));
    Serial.println(millis());
  }
  else
    Serial.println(F("Can't set ITimer1. Select another freq. or timer"));
  sei(); // allow interrupts
}
void loop()
{
  StaircaseChaser();
}

void StaircaseChaser()
{
  // set all to black
  fill_solid(leds, NUM_LEDS, CRGB::Black);

  // go thru all motion zones and light up according to decay value
  for (int i = 0; i < MOTION_ZONES; i++)
  {
    if (motions[i] > 0)
    {
      Lightup_Zone(i, motions[i]);
    }
  }
  FastLED.show();
  delay(50);
}

void Lightup_Zone(int zone, int intensity)
{
  for (int i = zone * zoneSize; i < (zone + 1) * zoneSize; i++)
  {
    leds[i] = ColorFromPalette(currentPalette, cIndex, 5 * intensity, currentBlending);
  }
}