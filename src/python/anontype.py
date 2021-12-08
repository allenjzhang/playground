class AnonObject(dict):
  __getattr__ = dict.get
  __setattr__ = dict.__setitem__

person = {
  "name": "michael",
  "age": 40
}
anonPerson = AnonObject(name="Aiden", age=14)

print(anonPerson)
print(anonPerson["name"])
print(anonPerson.name)