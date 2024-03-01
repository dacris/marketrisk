const http = require("http");
const { exec, spawn, execFile, execFileSync } = require('child_process');
var qs = require('querystring');
const fs = require('fs');
const mrroot = __dirname + "/..";

const PORT = process.env.PORT || 5000;

const server = http.createServer(async (req, res) => {
    //set the request route
    console.log(req.url);
    if (req.url === "/marketrisk" && req.method === "GET") {
       var data = fs.readFileSync(mrroot + "/LinuxUI/UI.html",
		"utf8");
	res.write(data);
	res.end();
    }
    else if (req.url === "/NewLogo.png" && req.method === "GET") {
	res.write(fs.readFileSync(mrroot + "/LinuxUI/NewLogo.png"));
	res.end();
    }
    else if (req.url === "/Combinations.json" && req.method === "GET") {
	res.write(fs.readFileSync(mrroot + "/Core/Combinations.json"));
	res.end();
    }
    else if (req.url === "/State.json" && req.method === "GET") {
	try {
	  if (fs.existsSync(mrroot + "/Core/State.json")) {
		//file exists
		res.write(fs.readFileSync(mrroot + "/Core/State.json"));
		res.end();
	  }
	  else {
		res.writeHead(404, { "Content-Type": "application/json" });
		res.end();
	  }
	} catch(err) {
	  console.error(err)
	}
    }
    else if (req.url === "/jquery.js" && req.method === "GET") {
	res.write(fs.readFileSync(mrroot + "/LinuxUI/jquery.js", "utf8"));
	res.end();
    }
    else if (req.url === "/mfp.js" && req.method === "GET") {
	res.write(fs.readFileSync(mrroot + "/LinuxUI/mfp.js", "utf8"));
	res.end();
    }
    else if (req.url === "/mfp.css" && req.method === "GET") {
	res.write(fs.readFileSync(mrroot + "/LinuxUI/mfp.css", "utf8"));
	res.end();
    }
    else if (req.url === "/Report.html" && req.method === "GET") {
	res.write(fs.readFileSync(mrroot + "/Core/Report.html", "utf8"));
	res.end();
    }
    else if (req.url === "/PortfolioPlanner.html" && req.method === "GET") {
	res.write(fs.readFileSync(mrroot + "/LinuxUI/PortfolioPlanner.html", "utf8"));
	res.end();
    }
    else if (req.url === "/marketrisk" && req.method === "POST") {
	var body = '';
        req.on('data', function (data) {
            body += data;
            // 1e6 === 1 * Math.pow(10, 6) === 1 * 1000000 ~~~ 1MB
            if (body.length > 1e6) { 
                // FLOOD ATTACK OR FAULTY CLIENT, NUKE REQUEST
                request.connection.destroy();
            }
        });
        req.on('end', function () {

        var POST = qs.parse(body);
        // use POST
	fs.writeFile(mrroot + '/Core/State.json',
		POST["state"], err => {
		  if (err) {
		    console.error(err);
		  }
		  console.log("save state");
		  // file written successfully
		  if (POST["report"] == "false") { return; }

	execFileSync(mrroot + '/Core/MarketRisk.Core.Console', [],
		{ cwd: mrroot + '/Core/' },
		(err, stdout, stderr) => {
		  if (err) {
		    console.error(err);
		    return;
		  }
		  console.log(stdout);
	});
	});
        });
        //end the response
        res.end();
    }

    // If no route present
    else {
        res.writeHead(404, { "Content-Type": "application/json" });
        res.end(JSON.stringify({ message: "Route not found" }));
    }
});

server.listen(PORT, () => {
    console.log(`server started on port: ${PORT}`);

var url = 'http://localhost:5000/marketrisk';
var start = (process.platform == 'darwin'? 'open': process.platform == 'win32'? 'start': 'xdg-open');
require('child_process').exec(start + ' ' + url);
});
