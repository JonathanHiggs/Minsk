function GreetingText(greeting: string, name: string): string
{
	return greeting + ", " + name + "!"
}

function Greet(greeting: string, name: string)
{
	var text = GreetingText(greeting, name)
	Print(text)
}