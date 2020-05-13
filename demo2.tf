provider "docker" {
  host = "tcp://127.0.0.1:2376/"
}

resource "docker_image" "ubuntu" {
  name = "ubuntu:latest"
}