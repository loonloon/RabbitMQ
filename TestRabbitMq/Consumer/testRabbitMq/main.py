import pika


def on_request(ch, method, props, body):
    content = body.decode()
    print(f"Received {content}")
    response = f"Processed {content}"
    ch.basic_publish(exchange='',
                     routing_key=props.reply_to,
                     properties=pika.BasicProperties(correlation_id=props.correlation_id),
                     body=str(response))
    ch.basic_ack(delivery_tag=method.delivery_tag)
    # ch.close()


connection = pika.BlockingConnection(pika.ConnectionParameters())
channel = connection.channel()
channel.basic_consume(queue='request_queue', on_message_callback=on_request, auto_ack=False)

print("Awaiting RPC requests")
channel.start_consuming()
