from http.server import BaseHTTPRequestHandler, HTTPServer

hostName = "localhost"
serverPort = 8080

class MyServer(BaseHTTPRequestHandler):
    state = "Stopped"
    
    def __init__(self, request, client_address, server):
        BaseHTTPRequestHandler.__init__(self, request, client_address, server)

    def do_GET(self):
        self.send_response(200)

        if(self.path == "/minecraft/start"):
            if(MyServer.state == "Stopped"):
                MyServer.state = "Starting"
        elif(self.path == "/minecraft/stop"):
            if(MyServer.state == "Running"):
                MyServer.state = "Stopping"
        else:
            if(MyServer.state == "Starting"):
                MyServer.state = "Running"
            if(MyServer.state == "Stopping"):
                MyServer.state = "Stopped"

        if (self.path == "/minecraft/status"):
            responseJson = '{"status" : "' + MyServer.state + '"}';
            self.send_header("Content-type", "application/json")
            self.end_headers()
            self.wfile.write(bytes(responseJson, "utf-8"));
        else:
            self.send_header("Content-type", "text/html")
            self.end_headers()
            self.wfile.write(bytes(MyServer.state, "utf-8"));

if __name__ == "__main__":        
    webServer = HTTPServer((hostName, serverPort), MyServer)
    print("Server started http://%s:%s" % (hostName, serverPort))

    try:
        webServer.serve_forever()
    except KeyboardInterrupt:
        pass

    webServer.server_close()
    print("Server stopped.")