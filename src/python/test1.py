nums = [2, 3, 4, 5, 7, 10, 12]

for n in nums:
  print(n, end=", ")

class CartItem:
  def __init__(self, name, price) -> None:
      self.price = price
      self.name = name

  def __repr__(self) -> str:
      return "({0}, ${1})".format(self.name, self.price)

class ShoppingCart:
  def __init__(self) -> None:
    self.items = []

  def add(self, cart_item):
    self.items.append(cart_item)

  def __iter__(self):
    return self.items.__iter__()

  @property
  def total_price(self):
    total = 0.0
    for item in self.items:
      total += item.price
    return total

print()
print()
cart = ShoppingCart()
cart.add(CartItem("CD", 9.99))
cart.add(CartItem("Vinyle", 14.99))

for c in cart:
  print(c)

print("Total is ${0:,}".format(cart.total_price))
