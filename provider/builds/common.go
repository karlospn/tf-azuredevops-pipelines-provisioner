package builds

import (
	"bytes"
	"errors"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
)

func send(client *api_client, method string, path string, data []byte) ([]byte, int, error) {

	fulluri := client.uri + path
	var req *http.Request
	var err error

	buffer := bytes.NewBuffer(data)

	req, err = http.NewRequest(method, fulluri, buffer)

	if err == nil {
		req.Header.Set("Content-Type", "application/json")
	}

	if err != nil {
		log.Fatal(err)
		return []byte{}, 500, err
	}

	if client.username != "" && client.password != "" {
		req.SetBasicAuth(client.username, client.password)
	}

	resp, err := client.http_client.Do(req)

	if err != nil {
		return []byte{}, resp.StatusCode, err
	}

	body, err2 := ioutil.ReadAll(resp.Body)
	resp.Body.Close()

	if err2 != nil {
		return []byte{}, resp.StatusCode, err2
	}

	if resp.StatusCode < 200 || resp.StatusCode >= 300 {
		return body, resp.StatusCode, errors.New(fmt.Sprintf("Unexpected response code '%d': %s", resp.StatusCode, body))
	}
	return body, resp.StatusCode, nil
}

func interface2StringList(configured []interface{}) []string {
	vs := make([]string, 0, len(configured))
	for _, v := range configured {
		val, ok := v.(string)
		if ok && val != "" {
			vs = append(vs, val)
		}
	}
	return vs
}

func interface2IntList(configured []interface{}) []int {
	vs := make([]int, 0, len(configured))
	for _, v := range configured {
		val, ok := v.(int)
		if ok {
			vs = append(vs, int(val))
		}
	}
	return vs
}
