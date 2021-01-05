function Main()
{
	let name = GetName()
	Print("Hello, " + name + "!")
}

function GetName(): string
{
	Print("What is your name?")
	return Input()
}