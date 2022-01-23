
# learningRabbitMq
I made these apps to apply what i have learnt in rabbitmq course
###
# simpleChatApp and ChatRooms:
      is for testing messaging between different instances of same app
# simpleChatApp: 
             used fanout exchange to publish messages to all active instances of app
###
# ChatRooms:
        is for testing chat rooms and every user select which room that he want to join and start messaging
####
# other apps for simulating tour booking system 
###
emailService and backendOffice acts as standAlone services that decoupled from tour booking system , i used message queue
###
to send messages to them when events happen in main system and they handle these meesages
