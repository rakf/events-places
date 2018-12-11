import {Component} from '@angular/core';
import {Router} from "@angular/router";
import {isNull} from "util";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  state = '';
  constructor(private router: Router) {

  }

  toRegister() {
    this.router.navigate(['app/register']);
  }

  setState(st: string) {
    this.state = st;
  }

  hideChoice() {
    return this.state.length !== 0 ;
  }

  cookieSet() {
    return isNull(this.getCookie("f_c"));
  }

  getCookie(name: string) {
    const nameLenPlus = (name.length + 1);
    return document.cookie
      .split(';')
      .map(c => c.trim())
      .filter(cookie => {
        return cookie.substring(0, nameLenPlus) === `${name}=`;
      })
      .map(cookie => {
        return decodeURIComponent(cookie.substring(nameLenPlus));
      })[0] || null;
  }
}
