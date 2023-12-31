﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

public class Post
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public DateTime CreateTime { get; set; }

    [Required]
    [MinLength(1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int ReadingTime { get; set; }

    [Url]
    public string? Image { get; set; }
    
    [Required]
    public Guid AuthorId { get; set; }

    [Required]
    [MinLength(1)]
    public string Author { get; set; } = string.Empty;
    
    public Guid? CommunityId { get; set; }

    public string? CommunityName { get; set; }
    
    public Guid? AddressId { get; set; }

    [Required] 
    public int Likes { get; set; }
    
    [Required]
    public int CommentsCount { get; set; }
    
    public List<Tag> Tags { get; set; } = new();

    [Required]
    public List<Comment> Comments { get; set; } = new();

    [Required]
    public List<Like> LikedPosts { get; set; } = new();

    public Community? Community { get; set; }
}