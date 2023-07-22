let map = fn(arr, f) {
  let iter = fn(arr, acc) {
    if (len(arr) == 0) { acc }
    else { iter(rest(arr), push(acc, f(first(arr)))); }
  };
  
  iter(arr, []);
};

let reduce = fn(arr, initial, f) {
  let iter = fn(arr, result) {
    if(len(arr) == 0) { result }
    else { iter(rest(arr), f(result, first(arr))); }
  };
  
  iter(arr, initial);
};

let sum = fn(arr) { reduce(arr, 0, fn(acc, el) { acc + el }); };
