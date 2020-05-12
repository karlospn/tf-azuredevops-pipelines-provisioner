data "http" "example" {
  url = "https://api.github.com/meta"

  request_headers {
    "Accept" = "application/json"
  }
}