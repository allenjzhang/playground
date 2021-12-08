import random

def test():
  days = ["Mon", "Tues", "Wed", "Thur", "Fri", "Sat", "Sun"]
  return days

def main():
  days = test()
  for day in days:
    rand = random.randint(0, 10)
    print("On {0} is {1}".format(day, rand))

if __name__ == "__main__":
  main()