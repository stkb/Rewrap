export default makeTest

function makeTest
  ( fn : () => () => void
  ): TestFn0
function makeTest<A>
  ( fn : (a: A) => () => void
  ): TestFn1<A>
function makeTest<A, B>
  ( fn : (a: A, b: B) => () => void
  ): TestFn2<A, B>
function makeTest<A, B, C>
  ( fn : (a: A, b: B, c: C) => () => void
  ): TestFn3<A, B, C>

function makeTest
  ( fn : (...args: any[]) => () => void
  ): TestFn
{
  const testFn = (title: string, ...args: any[]) => {
    test(title, fn.apply(this, args))
  }
  (<any>testFn).skip = (title: string, ...args: any[]) => {
    test.skip(title, fn.apply(this, args))
  }
  (<any>testFn).only = (title: string, ...args: any[]) => {
    test.only(title, fn.apply(this, args))
  }
  
  return <TestFn>testFn
}

interface TestFn {
  (title: string, ...args: any[]): void
  only(title: string, ...args: any[]): void
  skip(title: string, ...args: any[]): void
}

interface TestFn0 {
  (title: string): void
  only(title: string): void
  skip(title: string): void
}

interface TestFn1<A> {
  <A>(title: string, a: A): void
  only<A>(title: string, a: A): void
  skip<A>(title: string, a: A): void
}

interface TestFn2<A, B> {
  <A, B>(title: string, a: A, b: B): void
  only<A, B>(title: string, a: A, b: B): void
  skip<A, B>(title: string, a: A, b: B): void
}

interface TestFn3<A, B, C> {
  <A, B, C>(title: string, a: A, b: B, c: C): void
  only<A, B, C>(title: string, a: A, b: B, c: C): void
  skip<A, B, C>(title: string, a: A, b: B, c: C): void
}