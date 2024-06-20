﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tabloid.Models;
using Tabloid.Utils;

namespace Tabloid.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IConfiguration _config;
        public CommentRepository(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        public List<Comment> GetAllCommentsByPostId(int postId)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"select c.id, c.postid, c.userprofileid, c.subject, c.content, c.createdatetime, 
                              p.Title, up.DisplayName                                         
                              FROM comment c 
                                        LEFT JOIN Post p ON c.postId = p.id 
                                        LEFT JOIN UserProfile up ON c.UserProfileId = up.Id
                                        WHERE p.id = @postid
                                        ORDER BY c.CreateDateTime";

                    cmd.Parameters.AddWithValue("@postid", postId);

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Comment> comments = new List<Comment>();

                    while (reader.Read())
                    {
                        Comment comment = new Comment()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PostId = reader.GetInt32(reader.GetOrdinal("postid")),
                            UserProfileId = reader.GetInt32(reader.GetOrdinal("userprofileid")),
                            Subject = reader.GetString(reader.GetOrdinal("subject")),
                            Content = reader.GetString(reader.GetOrdinal("content")),
                            CreateDateTime = reader.GetDateTime(reader.GetOrdinal("createdatetime")),
                            UserProfile = new UserProfile()
                            {
                                DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                            },
                            Post = new Post()
                            {
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                            }
                        };
                        comments.Add(comment);

                    }
                    reader.Close();
                    return comments;

                }
            }
        }

        public void CreateComment(Comment comment)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())

                {
                    cmd.CommandText = @"
                            INSERT INTO Comment (PostId, UserProfileId, Subject, Content, CreateDateTime)
                            OUTPUT Inserted.Id
                            VALUES (@postId, @userProfileId, @subject, @content, @createDateTime); ";
                    cmd.Parameters.AddWithValue("@postId", comment.PostId);
                    cmd.Parameters.AddWithValue("@userProfileId", comment.UserProfileId);
                    cmd.Parameters.AddWithValue("@subject", comment.Subject);
                    cmd.Parameters.AddWithValue("@content", comment.Content);
                    cmd.Parameters.AddWithValue("@createDateTime", DateTime.Now);

                    int id = (int)cmd.ExecuteScalar();

                    comment.Id = id;
                }

            }

        }

        public void DeleteComment(int commentId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())

                {
                    cmd.CommandText = @"
                               DELETE FROM Comment
                               WHERE Id= @id";
                    cmd.Parameters.AddWithValue("@id", commentId);
                    cmd.ExecuteNonQuery();

                }
            }
        }
        public Comment GetCommentById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT Id, postid, userprofileid, subject, content, createdatetime 
                            FROM Comment
                        WHERE Id=@id";

                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        Comment comment = new Comment()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PostId = reader.GetInt32(reader.GetOrdinal("postid")),
                            UserProfileId = reader.GetInt32(reader.GetOrdinal("userprofileid")),
                            Subject = reader.GetString(reader.GetOrdinal("subject")),
                            Content = reader.GetString(reader.GetOrdinal("content")),
                            CreateDateTime = reader.GetDateTime(reader.GetOrdinal("createdatetime"))

                        };
                        reader.Close();
                        return comment;
                    }
                    else
                    {
                        reader.Close();
                        return null;
                    }
                }
            }
        }
        public void UpdateComment(Comment comment)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Comment
                                             SET
                                              postid = @postid,
                                              userprofileid = @userprofileid, 
                                              subject = @subject, 
                                              content = @content, 
                                              createdatetime = @createdatetime
                                             WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@postid", comment.PostId);
                    cmd.Parameters.AddWithValue("@userprofileid", comment.UserProfileId);
                    cmd.Parameters.AddWithValue("@subject", comment.Subject);
                    cmd.Parameters.AddWithValue("@content", comment.Content);
                    cmd.Parameters.AddWithValue("@createdatetime", comment.CreateDateTime);
                    cmd.Parameters.AddWithValue("@id", comment.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
