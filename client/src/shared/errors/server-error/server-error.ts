import { Component } from '@angular/core';
import { ApiError } from '../../../types/error';

@Component({
  selector: 'app-server-error',
  templateUrl: './server-error.html',
})
export class ServerError {
  protected error: ApiError;
  protected showDetails = false;

  constructor() {
    this.error = history.state?.error ?? null;
  }

  detailsToggle() {
    this.showDetails = !this.showDetails;
  }
}