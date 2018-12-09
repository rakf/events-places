import {Component} from "@angular/core";
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {Router} from "@angular/router";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})

export class RegisterComponent {
  public username: string;
  public password: string;
  private baseUrl = '/api/register';
  private headers: HttpHeaders = new HttpHeaders({'Content-Type': 'application/x-www-form-urlencoded'});
  constructor(private http: HttpClient, private router: Router) {

  }

  login(): void {
    const params = new URLSearchParams();
    params.set('username', this.username);
    params.set('password', this.password);

    this.http.post(this.baseUrl, params.toString(), {headers: this.headers, withCredentials: true}).subscribe(data => {
      if (data['response']['status'] === 'success') {
        this.setCookie("f_c", data['response']['cookie']);
        this.router.navigate(['/app'])
      } else {
        const s = document.createElement('script');
        s.type = 'text/javascript';
        s.innerHTML = 'alert(\'Регистрация не удалась\');';
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
