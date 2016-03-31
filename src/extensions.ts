Array.range = function(from: number, to: number, step = 1): number[] {
  const array = [] as number[]
  for(let i = from; i < to; i += step) {
    array.push(i)
  }
  return array
}

Array.prototype.apply = function(fn: (items: any[]) => any): any {
  return fn(this)
}

Array.prototype.flatMap = 
  function<T, U>(callback: (value: T, index: number, array: T[]) => U[]) : U[]
  {
    return (this as T[])
      .reduce((acc, v, i, arr) => acc.concat(callback(v, i, arr)), [] as U[])
  }