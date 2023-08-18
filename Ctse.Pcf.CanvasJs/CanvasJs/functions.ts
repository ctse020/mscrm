export function executeFunctionByName(functionName: string, context: any, ...args: any) {
    let args2 = Array.prototype.slice.call(arguments, 2);
    let namespaces = functionName.split(".");
    let func = namespaces.pop();
    for(let i = 0; i < namespaces.length; i++) {
      context = context[namespaces[i]];
    }
  
    if (context[func] == undefined) {
      throw 'function '+functionName+' not found'
    }
  
    try {
      return context[func].apply(context, args2);
    } catch(err) {
      throw err;
    }
  }

  export function executeJs(js: string) {
    return new Function(js)();
  }