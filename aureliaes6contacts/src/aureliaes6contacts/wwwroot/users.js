import {inject} from 'aurelia-framework'
import {HttpClient} from 'aurelia-fetch-client'

@inject(HttpClient)
export class Users {
    constructor(http) {
        this.heading = 'Github Users';
        this.users = [];
        http.configure(config => {
            config.useStandardConfiguration()
                .withBaseUrl('https://api.github.com/');
        });
        this.http = http;
    }

    activate() {
        return this.http.fetch('users')
            .then(response => {
                response.json().then(users => this.users = users);
            });
    }
}