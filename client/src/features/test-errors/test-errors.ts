import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  imports: [],
  templateUrl: './test-errors.html',
  styleUrl: './test-errors.css',
})
export class TestErrors {
  private http= inject(HttpClient);
  baseURL="https://localhost:5001/api"
  validationErrors=signal<string[]>([]);
  get404Error(){
    this.http.get(this.baseURL+'/buggy/not-found').subscribe({
      next: response=> console.log(response),
      error : error => console.log(error)
    })
  }
  get400Error(){
    this.http.get(this.baseURL+'/buggy/bad-request').subscribe({
      next: response=> console.log(response),
      error : error => console.log(error)
    })
  }
  get500Error(){
    this.http.get(this.baseURL+'/buggy/server-error').subscribe({
      next: response=> console.log(response),
      error : error => console.log(error)
    })
  }
  get401Error(){
    this.http.get(this.baseURL+'/buggy/auth').subscribe({
      next: response=> console.log(response),
      error : error => console.log(error)
    })
  }
  get400ValidationError(){
    this.http.post(this.baseURL+'/account/register',{}).subscribe({
      next: response=> console.log(response),
      error : error => {
        console.log(error)
        this.validationErrors.set(error);
      }
    })
  }
}
