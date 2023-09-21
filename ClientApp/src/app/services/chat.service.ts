import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { enviroment } from 'src/enviroments/enviroments';
import { User } from '../models/user';
import { HubConnection } from '@microsoft/signalr';
import { HubConnectionBuilder } from '@microsoft/signalr/dist/esm/HubConnectionBuilder';
import { Message } from '../models/message';
import { NgModel } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { PrivateChatComponent } from '../private-chat/private-chat.component';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  myName: string = '';
  private chatConnection?: HubConnection;
  onlineUsers: string[] = [];
  messages: Message[] = [];
  privateMessages: Message[] = [];
  privateMessageInitiated = false;


  constructor(private httpClient: HttpClient, private modalService: NgbModal) { }

  registerUser(user: User) {
    return this.httpClient.post(`${enviroment.apiUrl}api/chat/registerUser`, user, { responseType: 'text' })
  }

  //Everything in this method is invoked from C# and keeps the signalR connection alive
  createChatConnection() {
    this.chatConnection = new HubConnectionBuilder()
      .withUrl(`${enviroment.apiUrl}hubs/chat`)
      .withAutomaticReconnect()
      .build();

    this.chatConnection.start().catch(error => {
      console.log('Starting chat connection error: ', error);
    });

    this.chatConnection.on('UserConnected', () => {
      this.addUserConnectionId();
    });

    this.chatConnection.on('OnlineUsers', (onlineUsers) => {
      this.onlineUsers = [...onlineUsers];
    });

    this.chatConnection.on('NewMessage', (newMessage: Message) => {
      this.messages = [...this.messages, newMessage];
    })

    this.chatConnection.on('OpenPrivateChat', (newMessage: Message) => {
      this.privateMessages = [...this.privateMessages, newMessage];
      this.privateMessageInitiated = true;
      const modalRef = this.modalService.open(PrivateChatComponent);
      modalRef.componentInstance.toUser = newMessage.from;
    })

    this.chatConnection.on('NewPrivateMessage', (newMessage: Message) => {
      this.privateMessages = [...this.privateMessages, newMessage];
    })

    this.chatConnection.on('ClosePrivateChat ', () => {
      this.privateMessageInitiated = false;
      this.privateMessages = [];
      this.modalService.dismissAll();
    })

  }

  stopChatConnection() {
    this.chatConnection?.stop().catch(error => {
      console.log('Stopping chat connection error: ', error);
    });
  }

  async addUserConnectionId() {
    //Invokes from api the adduserconnectionId method  to add user connectionId
    return this.chatConnection?.invoke(`AddUserConnectionId`, this.myName).catch(error => {
      console.log('Add user error: ', error); 
    });
  }

  async sendMessage(content: string) {  
    const message: Message = {
      from: this.myName,
      content
    };

    return this.chatConnection?.invoke('RecieveMessage', message)
      .catch(error => console.log('Send message error: ', error));
  }

  async sendPrivateMessage(to: string, content: string) {
    const message: Message = {
      from: this.myName,
      to,
      content
    };

    if (!this.privateMessageInitiated) {
      this.privateMessageInitiated = true;
      return this.chatConnection?.invoke('CreatePrivateChat', message).then(() =>
      {
        this.privateMessages = [...this.privateMessages, message];
      })
        .catch(error => console.log('create private chat error: ', error));
    }
    else{
      
    return this.chatConnection?.invoke('ReceivePrivateMessage', message)
    .catch(error => console.log('Send private message error: ', error));
    }
  }

  async closePrivateChatMessage(otherUser: string) {
    this.privateMessageInitiated = false;
    return this.chatConnection?.invoke('RemovePrivateChat', this.myName, otherUser)
      .catch(error => console.log('Remove private chat error: ', error));

  }
}
