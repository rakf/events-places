import { Component } from '@angular/core';
import {Router} from "@angular/router";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'events-places';
  constructor(private router: Router) {

  }

  toLogin() {
    this.router.navigate(['app/login']);
  }

  toRegister() {
    this.router.navigate(['app/register']);
  }

}
