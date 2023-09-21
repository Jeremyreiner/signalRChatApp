import { Component, OnInit } from '@angular/core';
import { Form, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ChatService } from '../services/chat.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit{
  userForm: FormGroup = new FormGroup({});
  submitted = false;
  openchat = false;
  apiErrorMessages: string[] = [];


  constructor(private formBuilder: FormBuilder, private chatService: ChatService ) {}

  ngOnInit(): void {
    this.initializeForm()
  }

  initializeForm(){
    this.userForm = this.formBuilder.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]]
    })
  }

  submitForm(){
    this.submitted = true;
    this.apiErrorMessages = [];

    if(this.userForm.valid){
      console.log('Submit Form Value: ',this.userForm.value)
      this.chatService.registerUser(this.userForm.value).subscribe({
        next: () => {
          this.openchat = true;
          this.chatService.myName = this.userForm.get('name')?.value;
          this.userForm.reset();
          this.submitted = false;
        },
        error: error => {
          if(typeof(error.error) !== 'object'){
            this.apiErrorMessages.push(error.error)
          }
        }
      })
    }
  }

  closeChat() {
    this.openchat = false;
  }

}
