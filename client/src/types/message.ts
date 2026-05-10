export type Message= {
  id: string
  senderId: string
  senderDisplayName: string
  senderImageUrl: string
  content: string
  recipientId: string
  recipientDisplayName: string
  recipientImageUrl: string
  dataRead: string
  messageSent: string
  currentUserSender?: boolean
}