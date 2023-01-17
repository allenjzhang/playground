let firstName:string;
firstName="Allen";

function testFun(name: string, isActive: boolean) : number {
  return 1;
}

export class Employee {
  name: string;

  constructor(name:string) {
    this.name = name;
  }
}

class Manager extends Employee {
  private stockGrant : number;

  constructor(name: string, grant: number){
    super(name);
    this.stockGrant = grant;
  }
}