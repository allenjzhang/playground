import pymongo

class AnonObject(dict):
  __getattr__ = dict.get
  __setattr__ = dict.__setitem__

class Person:
  def __init__(self, name, age, hobby) -> None:
      self.hobby = hobby
      self.name = name
      self.age = age

  def __repr__(self) -> str:
      return "{0} is {1} and likes {2}".format(self.name, self.age, self.hobby)

people = [
  Person("jeff", 50, "Biking"),
  Person("Mike", 40, "Running"),
  Person("Aiden", 15, "Gaming"),
  Person("Allen", 50, "TV")
]

boomer = [
  AnonObject(Name=p.name, PastTime=p.hobby)
  for p in people
  if p.age >= 40
]
boomer.sort(key= lambda p: p.Name)

for b in boomer:
  print(b.Name)
