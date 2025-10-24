console.log("Hello, world");

// This file intentionally contains bad TypeScript code for LLM code review testing

function add(a, b) {
  return a + b;
}

let result = add("5", 10);

console.log("Result is: " + result);

const obj = {};
obj = { name: "test" };

if ((result = 15)) {
  console.log("Fifteen!");
}

let unusedVar: number;

function doNothing() {
  // does nothing
}
