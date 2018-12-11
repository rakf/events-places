import {Component} from "@angular/core";
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {Router} from "@angular/router";
import {User} from "../datamodels/user";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})

export class LoginComponent {
  user = new User('', '', '');
  private baseUrl = '/api/login';
  private headers: HttpHeaders = new HttpHeaders({'Content-Type': 'application/x-www-form-urlencoded'});
  constructor(private http: HttpClient, private router: Router) {

  }

  readyToSend() {
    return this.user.username.length !== 0 && this.user.password.length !== 0;
  }

  login(): void {
    const params = new URLSearchParams();
    params.set('username', this.user.username);
    params.set('password', this.user.password);
    console.log(this.user);

    this.http.post(this.baseUrl, params.toString(), {headers: this.headers, withCredentials: true}).subscribe(data => {
      if (data['response']['status'] === 'success') {
        this.setCookie("f_c", data['response']['cookie']);
        this.router.navigate(['/app'])
      } else {
        const s = document.createElement('script');
        s.type = 'text/javascript';
        s.innerHTML = 'alert(\'Такого пользователя нет\');';
        document.body.appendChild(s);
      }
    })
  }

  setCookie(name: string, val: string) {
    const date = new Date();
    const value = val;

    date.setTime(date.getTime() + 24 * 60 * 60 * 1000);
    document.cookie = name + "=" + value + "; expires =" + date.toUTCString() + "; path=/"
  }

}
