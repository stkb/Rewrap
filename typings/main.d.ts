/// <reference path="main/ambient/mz/index.d.ts" />

interface ArrayConstructor
{
  range(from: number, to: number, step?: number): number[]
}

interface Array<T> 
{
  apply<U>(callback: (items: T[]) => U): U
  
  flatMap<U>(callback: (value: T, index: number, array: T[]) => U[]) : U[]
}