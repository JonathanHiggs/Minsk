function GreetingText(greeting: string, name: string): string
{
	return greeting + ", " + name + "!"
}

function Greet(greeting: string, name: string)
{
	var text = GreetingText(greeting, name)
	Print(text)
}

let greeting = "Hello"

Print("What is your name?")
let name = Input()

Greet(greeting, name)
