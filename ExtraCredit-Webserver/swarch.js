var dgram = require('dgram');
var server = dgram.createSocket('udp4');
var fileSystem = require('fs');
var highScores;
var allTimeHigh;



fileSystem.readFile("HighScores.txt", 'utf8', function(err,data) {
	if(err){
        //Error occurred loading file, spit out error message then die
        console.log("Error occurred loading file");
        process.exit();
    }

    console.log("Loading high scores from file");

    try{
        // use JSON to turn file contents back into a Javascript array object
        highScores = JSON.parse(data);
    }catch(e)
    {
        // Exception occurred parsing file contents as JSON, error out and die.
        console.log("Exception occured parsing data");
        process.exit();
    }

    highScores.Scores.sort(function(a,b)
    {
    	return b.Score - a.Score;
    });
});

fileSystem.readFile("AllTimeHighScores.txt", 'utf8', function(err,data) {
    if(err){
        //Error occurred loading file, spit out error message then die
        console.log("Error occurred loading file");
        process.exit();
    }

    console.log("Loading high scores from file");

    try{
        // use JSON to turn file contents back into a Javascript array object
        allTimeHigh = JSON.parse(data);
    }catch(e)
    {
        // Exception occurred parsing file contents as JSON, error out and die.
        console.log("Exception occured parsing data");
        process.exit();
    }

    allTimeHigh.Scores.sort(function(a,b)
    {
        return b.Score - a.Score;
    });
});





console.log('Socket created\n');

server.on("message", function (msg, rinfo) {
	// body...
	console.log("Received message " + msg.toString());

	var s = msg.toString().split('/');

	console.log(s[0]);
	console.log(s[1]);
	console.log(s[2]);

	/*switch(s[0])
	{
		case "add":
			console.log("added score");
			break;

	}*/

	if(s[0] == "add")
	{
		console.log("added score");
		if(highScores !== undefined)
		{

			highScores.Scores.push({"Name" : s[1], "Score" : s[2]});
			var stringScore = JSON.stringify(highScores);

			fileSystem.writeFile("HighScores.txt", stringScore, function(err){
				if(err)
				{
					console.log("You suck");
					throw err;
				}
				console.log("Success!");



			});

                        // Loop through current highScores ( which should be sorted )
                        // and insert score if a lower match found
        }

        console.log(highScores.Scores);

    }

    else if(s[0] == "update")
    {
    	
        console.log("update scores");
    	if(highScores !== undefined)
    	{
    		for(i=0;i < highScores.Scores.length;++i)
    		{
    			if(highScores.Scores[i].Name == s[1])
    			{

    				console.log("found You!");
    				//highScores.Scores.push({"Name" : s[1], "Score" : s[2]});
					highScores.Scores[i].Score = s[2];
					var stringScore = JSON.stringify(highScores);
					fileSystem.writeFile("HighScores.txt", stringScore, function(err){
						if(err)
						{
							console.log("You suck");
							throw err;
						}
						console.log("Success!");



					});

					console.log(highScores.Scores);
					break;
				}
    		}
    		// Sort the array so it's in order}
    		highScores.Scores.sort(function(a,b)
    		{
    			return b.Score - a.Score;
    		});
    	}
    }

    else if(s[0] == "remove")
    {
        console.log("In remove");
        if(highScores !== undefined)
        {
            for(i = 0; i < highScores.Scores.length; ++i)
            {
                if(highScores.Scores[i].Name == s[1])
                {
                    var index = highScores.Scores.indexOf(highScores.Scores[i]);
                    console.log(index);
                    highScores.Scores.splice(index, 1);
                    var stringScore = JSON.stringify(highScores);

                    fileSystem.writeFile("HighScores.txt", stringScore, function(err){
                        if(err)
                        {
                            console.log("You suck");
                            throw err;
                        }
                        console.log("Success!");



                    });
                }
            }
        }

    }

    else if(s[0] == "gameover")
    {
        for(i = 0; i < highScores.Scores.length; i++)
        {
            allTimeHigh.Scores.push({"Name" : highScores.Scores[i].Name, "Score" : highScores.Scores[i].Score});
            
            
        }
        var stringScore = JSON.stringify(allTimeHigh);
        fileSystem.writeFile("AllTimeHighScores.txt", stringScore, function(err){
            if(err)
            {
                console.log("You suck");
                throw err;
            }
            console.log("Success!");



        });
        allTimeHigh.Scores.sort(function(a,b)
            {
                return b.Score - a.Score;
            });

        for(i = 0; i < highScores.Scores.length; i++)
        {
            highScores.Scores.splice(i, 2);

        }
        highScores.Scores.push({"Name" : "", "Score" : ""});

    }

    

});

// Called when socket starts listening for packets. besides logging, currently serves no purpose
server.on("listening", function () {
    var address = server.address();
    console.log("server listening " +
        address.address + ":" + address.port);
});


server.bind(9000);

// Now, lets show off a bit, with the worlds simplest web server.
// Dynamically creates webpage showing current high scores.
var webServer = require('http');


webServer.createServer(function(req,res){

   res.writeHead(200, {'Content-Type': 'text/html'});
   res.write('<meta http-equiv="refresh" content="1">');
   res.write("<html><head><title>SwarchServer</title></head><body> <h1>Live Game!</h1><ul>");
    
    for(i=0;i < highScores.Scores.length;++i)
    {
        res.write(highScores.Scores[i].Name + "&nbsp;&nbsp;" + highScores.Scores[i].Score + "<br />");
    }
    
    res.write("<html><head><title>SwarchServer</title></head><body> <h1 align = left >All Time High Scores</h1><ol>");
    for(i=0, j=0;i < allTimeHigh.Scores.length;++i)
    {
        if(allTimeHigh.Scores[i].name !== "")
        {
            res.write((j+1) + ") " + allTimeHigh.Scores[i].Name + "&nbsp;&nbsp;" + allTimeHigh.Scores[i].Score + "<br />");
            j++;
        }
    }
        
    
   res.write("</ul></body></html>");
   res.end();


}).listen(4111, '127.0.0.1');



