# Simple crontab
Welcome and thanks for choosing this project out of the various implementations! This library is a simple implementation of a crontab string parser, its only job is to receive a string that follows the crontab format (see https://crontab.guru/ for an amazing editor) and return the time in milliseconds to the closest execution.
As of writing of this readme only these [Commands](#commands) are available for use (which should cover most cases to be fair), but still if you have any suggestions for other commands you can open an issue on GitHub and I'll have a look at it.

## Capabilities
What does this project do and not do:

Does:
- Split and parse a crontab format string
- Returns the time in milliseconds to the closest execution time
- Does this very fast

Doesn't:
- Queue code executions
- Solve all your problems magically
- Pay for my bills

## Advantages
What advantages does using this implementation over others provide?
Well simple:
- It's a lightweight implementation
- It's fast
- Does not have any external dependencies
- You can decide what to do with the output instead of being locked to a Timer

## Disadvantages
Well, due to it being a basic implementation if you need something that already has a timer or has some more advanced features (like automatically executing a method after a certain time) you might be better off using another implementation

## Commands
Here's the list of the current crontab commands available to use on your strings:
- \* : Allows for all values
- , : Allows for multiple specific values given
- \- : Specifies a range
- / : Specifies the value is divisible by a certain number (example: \*/5, every number divisible by 5, so 0,5,10...)

## How to use
Easy! Just:
- Import the package (either from nuget.org or manually installed) 
- Add the using to your desired class (something like "using Crontab")
- And call Crontab.Crontab.CrontabTimeParser(\<YOUR-STRING-HERE>);

## Roadmap
Currently I'm not expecting to add anything specifically, I'll be waiting to see what bugs you find and what ideas are given to improve the project

## Technical data
Things to keep in mind about the project: 

- Due to the delay between the result of the crontab being given and your implementation using that data your timer might be off by a little bit, to get the most accurate execution try to use the CrontabTimeParser method as close as possible to when you're gonna use that data
- Days of week start as Sunday = 0 because of the DateTime.DayOfWeek enum implementation, that also means that the crontab string has the same format as typical implementations
- If you're gonna use that time to create a C# timer, its max size is 2,147,483,647 or about 25 days, so check the result from the crontab parsing or the Timer will throw an error ;)
- Yes I do understand that my code is not very easy to read or is visually appealing to most, but that's how my brain works so ![Cope](https://c.tenor.com/KvuKMxbmqwAAAAAC/tenor.gif)

## Disclaimer
This software is provided "as is" without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.