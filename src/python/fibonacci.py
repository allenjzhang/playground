memMap = {}

def fibonacci (n):
  if (n not in memMap):  
    if n <= 0:
      print("Invalid input")
    elif n == 1:
      memMap[n] = 0
    elif n == 2:
      memMap[n] = 1
    else:
      memMap[n] = fibonacci (n-1) + fibonacci (n-2)
  
  return memMap[n]

def fibonacciSlow (n):
  if n <= 0:
    print("Invalid input")
  elif n == 1:
    return 0
  elif n == 2:
    return 1
  else:
    return fibonacci (n-1) + fibonacci (n-2)

print(fibonacci (1000))
print("---------------")
print(fibonacciSlow (1000))